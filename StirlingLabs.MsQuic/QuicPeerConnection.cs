using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

namespace StirlingLabs.MsQuic;

[PublicAPI]
public abstract class QuicPeerConnection : IDisposable
{
    protected QuicRegistration Registration { get; }

    protected unsafe QUIC_HANDLE* _handle;

    public unsafe QUIC_HANDLE* Handle
    {
        get => _handle;
        protected set => _handle = value;
    }

    protected GCHandle GcHandle;

    private readonly ConcurrentDictionary<QuicStream, Nothing?> _streams = new();

    private readonly ConcurrentQueue<QuicStream> _unhandledStreams = new();

    protected int RunState;
    protected Memory<byte> _resumptionTicket;
    protected Memory<byte> _resumptionState;

    public ushort IdealProcessor { get; protected set; }
    public bool DatagramsAllowed { get; protected set; }
    public ushort MaxSendLength { get; protected set; }

    public bool IsResumed { get; protected set; }
    public SizedUtf8String NegotiatedAlpn { get; protected set; }
    public SizedUtf8String ServerName { get; protected set; }

    public IPEndPoint LocalEndPoint { get; protected set; }
    public IPEndPoint RemoteEndPoint { get; protected set; }

    public ushort AllowedBidirectionalStreams { get; protected set; }
    public ushort AllowedUnidirectionalStreams { get; protected set; }

    public bool LimitingRemoteStreams { get; protected set; }

    public bool ReceiveDatagramsAsync { get; set; }

    public X509Certificate2 Certificate { get; protected set; }

    public SignedCms CertificateChain { get; protected set; }

    protected QuicPeerConnection(QuicRegistration registration)
    {
        Registration = registration;
        GcHandle = GCHandle.Alloc(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool AddStream(QuicStream stream)
    {
        // ReSharper disable once InvertIf
        if (Interlocked.CompareExchange(ref RunState, 0, 0) != 0
            && _streams.TryAdd(stream, null))
        {
            _unhandledStreams.Enqueue(stream);
            return true;
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

    public unsafe void Shutdown()
    {
        if (Registration.Disposed) return;
        Registration.Table.ConnectionShutdown(Handle, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 0);
    }

    public unsafe void Close()
    {
        if (Registration.Disposed) return;
        Registration.Table.ConnectionClose(Handle);
        Handle = null;
    }


    public QuicDatagram SendDatagram(Memory<byte> data)
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

    public unsafe QuicDatagram SendDatagram(QuicDatagram dg)
    {
        if (dg is null) throw new ArgumentNullException(nameof(dg));

        var pBuf = dg.GetBuffer();

        var status = Registration.Table.DatagramSend(Handle, pBuf, 1, dg.Flags, (void*)GCHandle.ToIntPtr(dg.GcHandle));

        if (status != QUIC_STATUS_PENDING)
            AssertSuccess(status);

        return dg;
    }
    public QuicStream OpenStream(bool notStarted = false)
    {
        var stream = new QuicStream(Registration, this);
        AddStream(stream);
        if (!notStarted)
            stream.Start();
        return stream;
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicPeerConnection, QuicStream>? IncomingStream;

    protected void OnIncomingStream(QuicStream stream)
    {
        Debug.Assert(IncomingStream is not null);
        IncomingStream?.Invoke(this, stream);
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event ReadOnlySpanEventHandler<QuicPeerConnection, byte>? DatagramReceived;

    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off")]
    protected void OnDatagramReceived(ReadOnlySpan<byte> data)
    {
        try
        {
            DatagramReceived?.Invoke(this, data);
        }
        catch (Exception ex)
        {
            OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
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
                        context.Item2(context.Item1, data);
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
                if (!AddStream(stream))
                {
                    stream.Reject();
                    Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Rejected");
                    return QUIC_STATUS_ABORTED;
                }
                OnIncomingStream(stream);
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Accepted");
                return QUIC_STATUS_SUCCESS;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_IDEAL_PROCESSOR_CHANGED: {
                ref var typedEvent = ref @event.IDEAL_PROCESSOR_CHANGED;
                IdealProcessor = typedEvent.IdealProcessor;
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} {{IdealProcessor={IdealProcessor}}}");
                return QUIC_STATUS_SUCCESS;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_DATAGRAM_RECEIVED: {
                ref var typedEvent = ref @event.DATAGRAM_RECEIVED;

                Trace.TraceInformation(
                    $"{TimeStamp.Elapsed} {TimeStamp.Elapsed} {this} {@event.Type} {{ReceiveDatagramsAsync={ReceiveDatagramsAsync}}}");

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

                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} {{QuicDatagram={{{dg}}}}}");

                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_DATAGRAM_STATE_CHANGED: {
                ref var typedEvent = ref @event.DATAGRAM_STATE_CHANGED;
                DatagramsAllowed = typedEvent.SendEnabled != 0;
                MaxSendLength = typedEvent.MaxSendLength;
                Trace.TraceInformation(
                    $"{TimeStamp.Elapsed} {this} {@event.Type} {{DatagramsAllowed={DatagramsAllowed},MaxSendLength={MaxSendLength}}}");
                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_LOCAL_ADDRESS_CHANGED: {
                ref var typedEvent = ref @event.LOCAL_ADDRESS_CHANGED;
                LocalEndPoint = sockaddr.Read(typedEvent.Address);
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} {{LocalEndPoint={LocalEndPoint}}}");
                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_PEER_ADDRESS_CHANGED: {
                ref var typedEvent = ref @event.LOCAL_ADDRESS_CHANGED;
                RemoteEndPoint = sockaddr.Read(typedEvent.Address);
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} {{RemoteEndPoint={RemoteEndPoint}}}");
                return QUIC_STATUS_SUCCESS;
            }
            // client only
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_RESUMPTION_TICKET_RECEIVED: {
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;
            }
            // server only
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_RESUMED: {
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_CONNECTED:
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT:
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_PEER:
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE:
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_STREAMS_AVAILABLE: {
                ref var typedEvent = ref @event.STREAMS_AVAILABLE;
                AllowedBidirectionalStreams = typedEvent.BidirectionalCount;
                AllowedUnidirectionalStreams = typedEvent.UnidirectionalCount;
                Trace.TraceInformation(
                    $"{TimeStamp.Elapsed} {this} {@event.Type} {{AllowedBidirectionalStreams={AllowedBidirectionalStreams},AllowedBidirectionalStreams={AllowedUnidirectionalStreams}}}");
                return QUIC_STATUS_SUCCESS;
            }
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_PEER_NEEDS_STREAMS: {
                LimitingRemoteStreams = true;
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} {{LimitingRemoteStreams={LimitingRemoteStreams}}}");
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

                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type}");

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
                this.Certificate = cert;
                this.CertificateChain = chainContainer;

                var status = OnCertificateReceived(cert, chainContainer, typedEvent.DeferredErrorFlags, typedEvent.DeferredStatus);

                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} {{Result={GetNameForStatus(status)}}}");

                return status;

            }

            default:
                // ???
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;
        }

        return QUIC_STATUS_SUCCESS;
    }

    public EventHandler<QuicPeerConnection>? ResumptionTicketReceived;

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
        Trace.TraceInformation($"{TimeStamp.Elapsed} {this} event CertificateReceived");
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
                QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_CONNECTION,
                QUIC_PARAM_CONN_RESUMPTION_TICKET,
                (uint)_resumptionTicket.Length,
                pTicket);
        }
    }

    public abstract void Dispose();

    public event EventHandler<QuicPeerConnection, ExceptionDispatchInfo>? UnobservedException;

    protected virtual void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{TimeStamp.Elapsed} {this} {arg.SourceException}");
        UnobservedException?.Invoke(this, arg);
    }
}
