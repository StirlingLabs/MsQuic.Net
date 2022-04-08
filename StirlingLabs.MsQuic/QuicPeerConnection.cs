using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;
using static Microsoft.Quic.MsQuic;
using NativeMemory = StirlingLabs.Native.NativeMemory;

namespace StirlingLabs.MsQuic;

/*
 * ack stream pair will be streams 0 (and 1?) unidirectional both ways
 * at ack time intervals encode to fit max dg send size (frame size needed?)
 * 
 */

[PublicAPI]
public abstract partial class QuicPeerConnection : IDisposable
{
    static QuicPeerConnection()
        => LogTimeStamp.Init();

    protected unsafe QUIC_HANDLE* _handle;

    private int _maxSendLength;

    public unsafe QUIC_HANDLE* Handle
    {
        get => _handle;
        protected set => _handle = value;
    }

    protected GCHandle GcHandle;

    private readonly ConcurrentDictionary<QuicStream, Nothing?> _streams = new();

    private readonly ConcurrentQueue<QuicStream> _incomingStreamsQueue = new();

    public int QueuedIncomingStreams => _incomingStreamsQueue.Count;

    protected int RunState;
    protected Memory<byte> _resumptionTicket;
    protected Memory<byte> _resumptionState;

    protected QuicPeerConnection(QuicRegistration registration, bool reliableDatagrams)
    {
        DatagramsAreReliable = reliableDatagrams;
        Registration = registration;
        GcHandle = GCHandle.Alloc(this);
    }

    public string? Name { get; set; }

    protected QuicRegistration Registration { get; }

    public ushort IdealProcessor { get; protected set; }
    public bool DatagramsAllowed { get; protected set; }

    public ushort MaxSendLength
    {
        get {
            var maxSendLength = Interlocked.CompareExchange(ref _maxSendLength, 0, 0);
            return checked((ushort)(DatagramsAreReliable
                ? maxSendLength - VarIntSqlite4.GetEncodedLength(_reliableIdCounter + 120)
                : maxSendLength));
        }

        protected set => Interlocked.Exchange(ref _maxSendLength, value);
    }

    public bool IsResumed { get; protected set; }
    public SizedUtf8String NegotiatedAlpn { get; protected set; }
    public SizedUtf8String ServerName { get; protected set; }

    public IPEndPoint LocalEndPoint { get; protected set; } = null!;
    public IPEndPoint RemoteEndPoint { get; protected set; } = null!;

    public ushort AllowedBidirectionalStreams { get; protected set; }
    public ushort AllowedUnidirectionalStreams { get; protected set; }

    public bool LimitingRemoteStreams { get; protected set; }

    public bool ReceiveDatagramsAsync { get; set; }

    public X509Certificate2 Certificate { get; protected set; } = null!;

    public SignedCms CertificateChain { get; protected set; } = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off")]
    protected bool AddStream(QuicStream stream, bool local = false)
    {
        if (local)
        {
            var result = _streams.TryAdd(stream, null);
            Debug.Assert(result);
            return result;
        }
        // ReSharper disable once InvertIf
        if (Interlocked.CompareExchange(ref RunState, 0, 0) != 0
            && _streams.TryAdd(stream, null))
            return true;
        try
        {
            throw new("Stream failed to be added due to invalid RunState.")
                { Data = { [typeof(QuicStream)] = stream } };
        }
        catch (Exception ex)
        {
            OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool RemoveStream(QuicStream stream)
        => _streams.TryRemove(stream, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ClearStreams()
        => _streams.Clear();

    protected bool AllowAddingStreams
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Interlocked.CompareExchange(ref RunState, 0, 0) != 0;
    }

    public unsafe void Shutdown(bool silent = false)
    {
        if (Registration.Disposed) return;

        Registration.Table.ConnectionShutdown(Handle,
            silent
                ? QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_SILENT
                : QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 0);
    }

    public unsafe void Close()
    {
        if (Registration.Disposed) return;
        Registration.Table.ConnectionClose(Handle);
        Handle = null;
    }


    public QuicDatagram SendDatagram(Memory<byte> data)
    {
        // TODO: reliable path
        if (DatagramsAreReliable)
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            if (MemoryMarshal.TryGetMemoryManager<byte, MemoryManager<byte>>(data, out var mgr))
            {
                var dg = new QuicDatagramOwnedManagedMemoryReliable(this, mgr);

                SendDatagram(dg);

                return dg;
            }
            else
#endif
            {
                var dg = new QuicDatagramManagedMemoryReliable(this, data);

                SendDatagram(dg);

                return dg;
            }
        }
        else
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            if (MemoryMarshal.TryGetMemoryManager<byte, MemoryManager<byte>>(data, out var mgr))
            {
                var dg = new QuicDatagramOwnedManagedMemory(this, mgr);

                SendDatagram(dg);

                return dg;
            }
            else
#endif
            {
                var dg = new QuicDatagramManagedMemory(this, data);

                SendDatagram(dg);

                return dg;
            }
        }
    }

    public unsafe QuicDatagram SendDatagram(QuicDatagram dg)
    {
        if (dg is null) throw new ArgumentNullException(nameof(dg));

        var pBuf = dg.GetBuffer();

        // reliable case:
        // @formatter:off
        if (dg is QuicDatagramReliable dgr) lock (dgr.Lock)
        { // @formatter:on
            if (!DatagramsAreReliable)
                throw new("Must use plain (non-reliable) datagrams for this connection.");

            var header = &pBuf[0];
            if (header->Buffer != null)
            {
                NativeMemory.Free(header->Buffer);
                header->Length = 0;
            }

            var id = dgr.Id;
            var size = (uint)VarIntSqlite4.GetEncodedLength(id);
            header->Buffer = (byte*)NativeMemory.New(size);
            header->Length = size;
            var check = VarIntSqlite4.Encode(id, header->Span);
            Debug.Assert(size == check);

            _reliableDatagramsSentUnacknowledged.Add(id, dgr);

            var status = Registration.Table.DatagramSend(Handle, pBuf, 2, dg.Flags, (void*)GCHandle.ToIntPtr(dg.GcHandle));

            if (status != QUIC_STATUS_PENDING)
                AssertSuccess(status);

            return dg;
        }

        // non-reliable case:
        {
            if (DatagramsAreReliable)
                throw new("Must use reliable datagrams for this connection.");

            var status = Registration.Table.DatagramSend(Handle, pBuf, 1, dg.Flags, (void*)GCHandle.ToIntPtr(dg.GcHandle));

            if (status != QUIC_STATUS_PENDING)
                AssertSuccess(status);

            return dg;
        }
    }
    public QuicStream OpenStream(bool notStarted = false)
    {
        var stream = new QuicStream(Registration, this);
        AddStream(stream, true);
        if (!notStarted)
            stream.Start();
        return stream;
    }
    public QuicStream OpenUnidirectionalStream(bool notStarted = false)
    {
        var stream = new QuicStream(Registration, this, QUIC_STREAM_OPEN_FLAGS.QUIC_STREAM_OPEN_FLAG_UNIDIRECTIONAL);
        AddStream(stream, true);
        if (!notStarted)
            stream.Start();
        return stream;
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    private event EventHandler<QuicPeerConnection, QuicStream>? _incomingStream;

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicPeerConnection, QuicStream>? IncomingStream
    {
        add {
            if (value is not null && Interlocked.CompareExchange(ref _incomingStream, value, null) is null)
                while (_incomingStreamsQueue.TryDequeue(out var stream))
                    value(this, stream);
            else
                _incomingStream += value;
        }
        remove => _incomingStream -= value;
    }

    protected void OnIncomingStream(QuicStream stream)
    {
        var eh = Interlocked.CompareExchange(ref _incomingStream, null, null);
        if (DatagramsAreReliable && InboundAcknowledgementStream is null)
        {
            Debug.Assert(eh is null);
            InboundAcknowledgementStream = stream;
            WireUpInboundAcknowledgementStream();
            Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} Incoming Inbound Acknowledgement Stream {stream}");
        }
        else
        {
            Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} Incoming Stream {stream}");
            if (eh is not null)
                eh.Invoke(this, stream);
            else
                _incomingStreamsQueue.Enqueue(stream);
        }
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event ReadOnlySpanEventHandler<QuicPeerConnection, byte>? DatagramReceived;

    private ConcurrentDictionary<ulong, Timestamp> _seenIds = new();
    private ConcurrentDictionary<ulong, Timestamp> _ackdIds = new();

    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off")]
    protected void OnDatagramReceived(ReadOnlySpan<byte> data)
    {
        try
        {
            Debug.Assert(data.Length > 0);
            if (!DatagramsAreReliable)
                DatagramReceived?.Invoke(this, data);
            else
            {
                var header = VarIntSqlite4.GetDecodedLength(data[0]);
                var id = VarIntSqlite4.Decode(data);
                if (!_ackdIds.TryGetValue(id, out var prevSeen))
                    _seenIds.AddOrUpdate(id, _ => Timestamp.Now,
                        (_, prevSeenVal) => {
                            prevSeen = prevSeenVal;
                            return Timestamp.Now;
                        });

                if (prevSeen == default)
                {
                    Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} Received Reliable Datagram #{id}");
                    DatagramReceived?.Invoke(this, data.Slice(header));
                }
                else
                    Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} Received Reliable Datagram #{id} [Retransmit {prevSeen}]");
            }
        }
        catch (Exception ex)
        {
            OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
        }
        finally
        {
            ReliableDatagramAcknowledgementInterval ??= new(
                ReliableDatagramAcknowledgementIntervalTime.TotalSeconds,
                GenerateReliableDatagramAcknowledgments);
        }
    }
    private bool GenerateReliableDatagramAcknowledgments()
    {
        lock (_reliableDatagramAckLock)
        {
            var ids = _seenIds.Keys.ToArray();
            foreach (var id in ids)
                if (_seenIds.TryGetValue(id, out var seen))
                    _ackdIds[id] = seen;
            var toAck = new SortedSet<ulong>(ids);
            if (TrySendDatagramAcks(toAck))
            {
                foreach (var id in ids)
                    if (!toAck.Contains(id))
                        _seenIds.TryRemove(id, out _);
                return true;
            }
            return true;
        }
    }

    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off")]
    protected void OnDatagramReceivedAsync(byte[] data)
    {
        if (DatagramReceived is not null)
        {
            var delegates = DatagramReceived.GetInvocationList();
            foreach (var dg in delegates)
            {
                ThreadPool.QueueUserWorkItem(state => {
                    var context = ((QuicPeerConnection, ReadOnlySpanEventHandler<QuicPeerConnection, byte>, byte[]))state!;
                    try
                    {
                        Debug.Assert(data.Length > 0);
                        if (!DatagramsAreReliable)
                            context.Item2(context.Item1, data);
                        else
                        {
                            var header = VarIntSqlite4.GetDecodedLength(data[0]);
                            var id = VarIntSqlite4.Decode(data);
                            Timestamp prevSeen = default;
                            _seenIds.AddOrUpdate(id, _ => Timestamp.Now,
                                (_, prevSeenVal) => {
                                    prevSeen = prevSeenVal;
                                    return Timestamp.Now;
                                });

                            var length = data.Length - header;

                            if (prevSeen == default)
                            {
                                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} Async Received Reliable Datagram #{id}");
                                context.Item2(context.Item1, new(data, header, length));
                            }
                            else
                                Trace.TraceInformation(
                                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} Async Received Reliable Datagram #{id} [Retransmit {prevSeen}]");
                        }
                    }
                    catch (Exception ex)
                    {
                        OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
                    }
                }, (this, dg, data));
            }
        }
        ArrayPool<byte>.Shared.Return(data);
    }

    [SuppressMessage("Design", "CA1045", Justification = "Native struct")]
    [SuppressMessage("Reliability", "CA2000", Justification = "Either GC will call Dispose or objects will be reused")]
    protected unsafe int DefaultManagedCallback(ref QUIC_CONNECTION_EVENT @event)
    {
        switch (@event.Type)
        {

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_PEER_STREAM_STARTED: {
                ref var typedEvent = ref @event.PEER_STREAM_STARTED;
                var stream = new QuicStream(Registration, this, typedEvent.Stream, typedEvent.Flags);
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Pending");
                if (!AddStream(stream))
                {
                    stream.Reject();
                    Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Rejected");
                    return QUIC_STATUS_ABORTED;
                }
                OnIncomingStream(stream);
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Accepted");
                return QUIC_STATUS_SUCCESS;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_IDEAL_PROCESSOR_CHANGED: {
                ref var typedEvent = ref @event.IDEAL_PROCESSOR_CHANGED;
                IdealProcessor = typedEvent.IdealProcessor;
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{IdealProcessor={IdealProcessor}}}");
                return QUIC_STATUS_SUCCESS;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_DATAGRAM_RECEIVED: {
                ref var typedEvent = ref @event.DATAGRAM_RECEIVED;

                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{ReceiveDatagramsAsync={ReceiveDatagramsAsync}}}");

                if (ReceiveDatagramsAsync)
                {
                    var bytes = ArrayPool<byte>.Shared.Rent((int)typedEvent.Buffer->Length);
                    fixed (byte* pBytes = bytes)
                        Unsafe.CopyBlock(pBytes, typedEvent.Buffer->Buffer, typedEvent.Buffer->Length);
                    OnDatagramReceivedAsync(bytes);
                    return QUIC_STATUS_SUCCESS;
                }

                // sync zero-copy path
                OnDatagramReceived(typedEvent.Buffer->ReadOnlySpan);
                return QUIC_STATUS_SUCCESS;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_DATAGRAM_SEND_STATE_CHANGED: {
                ref var typedEvent = ref @event.DATAGRAM_SEND_STATE_CHANGED;

                var dg = (QuicDatagram?)GCHandle.FromIntPtr((IntPtr)typedEvent.ClientContext).Target!;

                dg.State = typedEvent.State;

                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{QuicDatagram={{{dg}}}}}");

                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_DATAGRAM_STATE_CHANGED: {
                ref var typedEvent = ref @event.DATAGRAM_STATE_CHANGED;
                DatagramsAllowed = typedEvent.SendEnabled != 0;
                MaxSendLength = typedEvent.MaxSendLength;
                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{DatagramsAllowed={DatagramsAllowed},MaxSendLength={MaxSendLength}}}");
                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_LOCAL_ADDRESS_CHANGED: {
                ref var typedEvent = ref @event.LOCAL_ADDRESS_CHANGED;
                LocalEndPoint = ((sockaddr*)typedEvent.Address)->ToEndPoint();
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{LocalEndPoint={LocalEndPoint}}}");
                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_PEER_ADDRESS_CHANGED: {
                ref var typedEvent = ref @event.LOCAL_ADDRESS_CHANGED;
                RemoteEndPoint = ((sockaddr*)typedEvent.Address)->ToEndPoint();
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{RemoteEndPoint={RemoteEndPoint}}}");
                return QUIC_STATUS_SUCCESS;
            }
            // client only
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_RESUMPTION_TICKET_RECEIVED: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Unhandled");
                break;
            }
            // server only
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_RESUMED: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Unhandled");
                break;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_CONNECTED:
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Unhandled");
                break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                ref var typedEvent = ref @event.SHUTDOWN_INITIATED_BY_TRANSPORT;
                OnShutdown(ref typedEvent);
                break;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_PEER: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                ref var typedEvent = ref @event.SHUTDOWN_INITIATED_BY_PEER;
                OnShutdown(ref typedEvent);
                break;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                ref var typedEvent = ref @event.SHUTDOWN_COMPLETE;
                OnShutdownComplete(ref typedEvent);
                break;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_STREAMS_AVAILABLE: {
                ref var typedEvent = ref @event.STREAMS_AVAILABLE;
                AllowedBidirectionalStreams = typedEvent.BidirectionalCount;
                AllowedUnidirectionalStreams = typedEvent.UnidirectionalCount;
                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{AllowedBidirectionalStreams={AllowedBidirectionalStreams},AllowedBidirectionalStreams={AllowedUnidirectionalStreams}}}");
                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_PEER_NEEDS_STREAMS: {
                LimitingRemoteStreams = true;
                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{LimitingRemoteStreams={LimitingRemoteStreams}}}");
                break;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_PEER_CERTIFICATE_RECEIVED: {
                ref var typedEvent = ref @event.PEER_CERTIFICATE_RECEIVED;

                var certBuf = (QUIC_BUFFER*)typedEvent.Certificate;
                var chainBuf = (QUIC_BUFFER*)typedEvent.Chain;

                var certSpan = certBuf->Span;

                var chainSpan = chainBuf->Span;

                var chainContainer = new SignedCms();
                chainContainer.Decode(chainSpan);

#if NET5_0_OR_GREATER
                var cert = new X509Certificate2(certSpan);
#else
                var certBytes = new byte[certSpan.Length];
                certSpan.CopyTo(certBytes);
                var cert = new X509Certificate2(certBytes);
#endif

                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");

#if TRACE_CERTIFICATES
                Trace.TraceInformation("===== BEGIN CERTIFICATE INFORMATION =====");
                Trace.TraceInformation(cert.ToString(true));
                Trace.TraceInformation("===== END CERTIFICATE INFORMATION =====");
                Trace.TraceInformation("===== BEGIN CERTIFICATE CHAIN INFORMATION =====");
                {
                    var i = 1;
                    foreach (var c in chainContainer.Certificates)
                    {
                        Trace.TraceInformation($"===== BEGIN CERTIFICATE {i} INFORMATION =====");
                        Trace.TraceInformation(c.ToString(true));
                        Trace.TraceInformation($"===== END CERTIFICATE {i} INFORMATION =====");
                        ++i;
                    }
                }
                Trace.TraceInformation("===== END CERTIFICATE CHAIN INFORMATION =====");
#endif
                Certificate = cert;
                CertificateChain = chainContainer;

                var status = OnCertificateReceived(cert, chainContainer, typedEvent.DeferredErrorFlags, typedEvent.DeferredStatus);

                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{Result={GetNameForStatus(status)}}}");

                return status;

            }

            default:
                // ???
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Unhandled");
                break;
        }

        return QUIC_STATUS_SUCCESS;
    }

    public Utilities.EventHandler<QuicPeerConnection>? ResumptionTicketReceived;

    protected void OnResumptionTicketReceived()
        => ResumptionTicketReceived?.Invoke(this);

    public delegate int CertificateReceivedEventHandler(
        QuicPeerConnection peer,
        X509Certificate2 certificate,
        SignedCms chain,
        uint deferredErrorFlags,
        int deferredStatus
    );

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event CertificateReceivedEventHandler? CertificateReceived;

    protected int OnCertificateReceived(
        X509Certificate2 cert,
        SignedCms chainContainer,
        uint deferredErrorFlags,
        int deferredStatus
    )
    {
        Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} event CertificateReceived");
        var status = CertificateReceived?.Invoke(
            this,
            cert,
            chainContainer,
            deferredErrorFlags,
            deferredStatus);
        return status ?? DefaultCertificateReceivedStatus;
    }

    protected abstract int DefaultCertificateReceivedStatus { get; }

    public Memory<byte> ResumptionTicket => _resumptionTicket;

    public Memory<byte> ResumptionState => _resumptionState;

    public unsafe void SetResumptionTicket(Memory<byte> resumptionTicket)
    {
        _resumptionTicket = resumptionTicket;
        fixed (byte* pTicket = _resumptionTicket.Span)
        {
            Registration.Table.SetParam(
                Handle,
                QUIC_PARAM_CONN_RESUMPTION_TICKET,
                (uint)_resumptionTicket.Length,
                pTicket);
        }
    }

    public abstract void Dispose();

    public event EventHandler<QuicPeerConnection, ExceptionDispatchInfo>? UnobservedException;

    protected internal virtual void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{LogTimeStamp.ElapsedSeconds:F6} {this} {arg.SourceException}");
        UnobservedException?.Invoke(this, arg);
    }

    protected QuicStream? InboundAcknowledgementStream { get; set; }
    protected QuicStream? OutboundAcknowledgementStream { get; set; }


    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicPeerConnection, bool, bool>? ConnectionShutdown;

    private unsafe void OnShutdown(
        ref QUIC_CONNECTION_EVENT._Anonymous_e__Union._SHUTDOWN_INITIATED_BY_TRANSPORT_e__Struct typedEvent)
    {
        static void Dispatcher(
            (
                QuicPeerConnection connection,
                ulong errorCode,
                EventHandler<QuicPeerConnection, ulong, bool, bool>[] handlers
                ) o)
        {
            foreach (var eh in o.handlers)
                eh.Invoke(o.connection, o.errorCode, true, false);
        }

        if (ConnectionShutdown is not null)
            ThreadPoolHelpers.QueueUserWorkItemFast(&Dispatcher,
                (
                    this,
                    unchecked((ulong)typedEvent.Status),
                    (EventHandler<QuicPeerConnection, ulong, bool, bool>[])ConnectionShutdown.GetInvocationList()
                )
            );
    }

    private unsafe void OnShutdown(
        ref QUIC_CONNECTION_EVENT._Anonymous_e__Union._SHUTDOWN_INITIATED_BY_PEER_e__Struct typedEvent)
    {
        static void Dispatcher(
            (
                QuicPeerConnection connection,
                ulong errorCode,
                EventHandler<QuicPeerConnection, ulong, bool, bool>[] handlers
                ) o)
        {
            foreach (var eh in o.handlers)
                eh.Invoke(o.connection, o.errorCode, false, true);
        }

        if (ConnectionShutdown is not null)
            ThreadPoolHelpers.QueueUserWorkItemFast(&Dispatcher,
                (
                    this,
                    typedEvent.ErrorCode,
                    (EventHandler<QuicPeerConnection, ulong, bool, bool>[])ConnectionShutdown.GetInvocationList()
                )
            );
    }


    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    // connection, appCloseInProgress, handshakeCompleted, peerAcknowledgedShutdown
    public event EventHandler<QuicPeerConnection, bool, bool, bool>? ConnectionShutdownComplete;

    private unsafe void OnShutdownComplete(
        ref QUIC_CONNECTION_EVENT._Anonymous_e__Union._SHUTDOWN_COMPLETE_e__Struct typedEvent)
    {
        static void Dispatcher(
            (QuicPeerConnection connection, EventHandler<QuicPeerConnection, bool, bool, bool>[] handlers,
                bool appCloseInProgress, bool handshakeCompleted, bool peerAcknowledged) o)
        {
            foreach (var eh in o.handlers)
                eh.Invoke(o.connection, o.appCloseInProgress, o.handshakeCompleted, o.peerAcknowledged);
        }

        if (ConnectionShutdownComplete is not null)
            ThreadPoolHelpers.QueueUserWorkItemFast(&Dispatcher,
                (this, (EventHandler<QuicPeerConnection, bool, bool, bool>[])
                    ConnectionShutdownComplete.GetInvocationList(),
                    typedEvent.AppCloseInProgress != 0,
                    typedEvent.HandshakeCompleted != 0,
                    typedEvent.PeerAcknowledgedShutdown != 0));
    }
}
