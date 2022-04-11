using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;
using static Microsoft.Quic.MsQuic;
using NativeMemory = StirlingLabs.Native.NativeMemory;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed partial class QuicStream : IDisposable
{
    private unsafe QUIC_HANDLE* _handle;
    public unsafe QUIC_HANDLE* Handle => _handle;

    private GCHandle _gcHandle;

    public readonly QuicPeerConnection Connection;

    public readonly QUIC_STREAM_OPEN_FLAGS Flags;

    private int _runState;

    private readonly object _lastRecvLock = new();
    private QUIC_RECEIVE_FLAGS _lastRecvFlags;
    private uint _lastRecvBufIndex;
    private uint _lastRecvBufOffset;
    private uint _lastRecvTotalRead;
    private unsafe QUIC_BUFFER* _lastRecvBufs;
    private uint _lastRecvBufsCount;

    //private Memory<byte> _unreadBuffer;
    private bool _started;

    public unsafe QuicStream(QuicRegistration registration, QuicPeerConnection connection,
        QUIC_STREAM_OPEN_FLAGS flags = QUIC_STREAM_OPEN_FLAGS.QUIC_STREAM_OPEN_FLAG_0_RTT)
    {
        _gcHandle = GCHandle.Alloc(this);
        Registration = registration;
        Connection = connection;

        QUIC_HANDLE* handle = null;
        try
        {
            var status = Registration.Table.StreamOpen(Connection.Handle,
                flags,
#if NET5_0_OR_GREATER
                &NativeCallback,
#else
                NativeCallbackThunkPointer,
#endif
                (void*)(IntPtr)_gcHandle, &handle);

            AssertSuccess(status);
        }
        catch
        {
            _gcHandle.Free();
            throw;
        }

        _handle = handle;

        _started = false;
    }

    internal unsafe QuicStream(QuicRegistration registration, QuicPeerConnection connection, QUIC_HANDLE* handle, QUIC_STREAM_OPEN_FLAGS flags)
    {
        _gcHandle = GCHandle.Alloc(this);
        Registration = registration;
        Connection = connection;

        _handle = handle;

        Flags = flags;

#if NET5_0_OR_GREATER
        delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, int> nativeCallback = &NativeCallback;
#else
        var nativeCallback = NativeCallbackThunkPointer;
#endif

        _started = true;
        Interlocked.Exchange(ref _runState, 0);

        registration.Table.SetCallbackHandler(handle, nativeCallback, (void*)(IntPtr)_gcHandle);
    }

    public string? Name { get; set; }

    public QuicRegistration Registration { get; }

    public unsafe long Id
    {
        get {

            var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
            if (rs <= 0) throw new InvalidOperationException("Stream is not initialized.");
            uint l = sizeof(long);
            long value = -1;
            var status = Registration.Table.GetParam(Handle, QUIC_PARAM_STREAM_ID, &l, &value);
            AssertSuccess(status);
            return value;
        }
        //set => Registration.Table.SetParam(Handle, QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_STREAM, QUIC_PARAM_STREAM_ID, 8, &value);
    }

    public unsafe bool TryGetId(out long id)
    {
        uint l = sizeof(long);
        fixed (long* pId = &id)
            return IsSuccess(Registration.Table.GetParam(Handle, QUIC_PARAM_STREAM_ID, &l, pId));
    }


    public ulong IdealSendBufferSize { get; private set; }


    public unsafe uint DataAvailable
    {
        get {
            var rs = Interlocked.CompareExchange(ref _runState, 0, 0);

            switch (rs)
            {
                case 0:
                    return 0;

                case -1:
                    //return (uint)_unreadBuffer.Length;
                    throw new ObjectDisposedException(nameof(QuicStream));
            }

            lock (_lastRecvLock)
            {
                if (_lastRecvBufIndex >= _lastRecvBufsCount)
                    return 0;

                var avail = _lastRecvBufs[_lastRecvBufIndex].Length - _lastRecvBufOffset;

                for (var i = _lastRecvBufIndex + 1; i < _lastRecvBufsCount; ++i)
                    avail += _lastRecvBufs[i].Length;

                return avail;
            }
        }
    }

#if !NET5_0_OR_GREATER
    private unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_STREAM_EVENT* @event);

    private static readonly unsafe NativeCallbackDelegate NativeCallbackThunk = NativeCallback;
    private static readonly unsafe delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, int> NativeCallbackThunkPointer
        = (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, int>)Marshal.GetFunctionPointerForDelegate(
            NativeCallbackThunk);
#endif


#if NET5_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    private static unsafe int NativeCallback(QUIC_HANDLE* handle, void* context, QUIC_STREAM_EVENT* @event)
    {
        var self = (QuicStream)GCHandle.FromIntPtr((IntPtr)context).Target!;
        return self.ManagedCallback(ref *@event);

    }

    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    private unsafe int ManagedCallback(ref QUIC_STREAM_EVENT @event)
    {

        switch (@event.Type)
        {
            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_RECEIVE: {
                ref var typedEvent = ref @event.RECEIVE;

                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{TotalBufferLength={typedEvent.TotalBufferLength}}}");

                Debug.Assert(_lastRecvBufIndex >= _lastRecvBufsCount);
                lock (_lastRecvLock)
                {
                    _lastRecvFlags = typedEvent.Flags;
                    _lastRecvBufs = typedEvent.Buffers;
                    _lastRecvBufsCount = typedEvent.BufferCount;
                    _lastRecvBufIndex = 0;
                    _lastRecvBufOffset = 0;
                    _lastRecvTotalRead = 0;
                    Interlocked.Exchange(ref _runState, 2);
                    OnDataReceived();
                }

                return DataAvailable == 0
                    ? QUIC_STATUS_SUCCESS
                    : QUIC_STATUS_PENDING;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_START_COMPLETE: {
                ref var typedEvent = ref @event.START_COMPLETE;
                var status = typedEvent.Status;
                if (IsSuccess(status))
                {
                    Interlocked.Exchange(ref _runState, 2);
                    _started = true;
                }
                else
                {
                    if (_started)
                    {
                        var ex = new InvalidOperationException("Remote party attempted to start the stream multiple times.");
                        Connection.OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
                        Connection.Shutdown(true);
                        Connection.Close();
                        return QUIC_STATUS_ABORTED;
                    }
                    Close();
                }
                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{Id={typedEvent.ID},Status={GetNameForStatus(status)},PeerAccepted={typedEvent.PeerAccepted},Started={_started}}}");
                OnStartComplete(status);
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_SEND_COMPLETE: {
                var pClientCtx = (IntPtr)@event.SEND_COMPLETE.ClientContext;
                var gchClientCtx = GCHandle.FromIntPtr(pClientCtx);
                var disposable = (IDisposable?)gchClientCtx.Target;
                if (disposable is SendContext sc)
                    sc.TaskCompletionSource.TrySetResult(true);
                disposable?.Dispose();
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_IDEAL_SEND_BUFFER_SIZE: {
                ref var typedEvent = ref @event.IDEAL_SEND_BUFFER_SIZE;
                IdealSendBufferSize = typedEvent.ByteCount;
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_SEND_SHUTDOWN_COMPLETE: {
                var graceful = @event.SEND_SHUTDOWN_COMPLETE.Graceful != 0;
                if (graceful)
                {
                    // TODO: ???
                }
                else
                {
                    // TODO: ???
                }
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {(graceful ? "Gracefully" : "Ungracefully")}");
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_SHUTDOWN_COMPLETE: {
                ref var typedEvent = ref @event.SHUTDOWN_COMPLETE;
                // TODO: ???
                var connectionShutdown = typedEvent.ConnectionShutdown != 0;
                if (connectionShutdown)
                    Interlocked.Exchange(ref _runState, -2);
                else
                    Interlocked.Exchange(ref _runState, -1);

                var appCloseInProgress = typedEvent.AppCloseInProgress != 0;

                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{ConnectionShutdown={typedEvent.ConnectionShutdown}}}");

                OnShutdownComplete(connectionShutdown, appCloseInProgress);
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_PEER_SEND_SHUTDOWN: {
                Interlocked.Exchange(ref _runState, -1);
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return 0;
            }
            default: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} Unhandled");
                throw new NotImplementedException(@event.Type.ToString());
            }
        }

    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    // stream, connectionShutdown, appCloseInProgress
    public event EventHandler<QuicPeerConnection, bool, bool>? ShutdownComplete;

    private unsafe void OnShutdownComplete(bool connectionShutdown, bool appCloseInProgress)
    {
        static void Dispatcher(
            (QuicStream stream, EventHandler<QuicStream, bool, bool>[] handlers,
                bool connectionShutdown, bool appCloseInProgress) o)
        {
            foreach (var eh in o.handlers)
                eh.Invoke(o.stream, o.connectionShutdown, o.appCloseInProgress);
        }

        if (ShutdownComplete is not null)
            ThreadPoolHelpers.QueueUserWorkItemFast(&Dispatcher,
                (this, (EventHandler<QuicStream, bool, bool>[])
                    ShutdownComplete.GetInvocationList(),
                    connectionShutdown,
                    appCloseInProgress));
    }

    public Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellation)
    {
        if (HasDataReceivedHandler)
            throw GetDataReceivedHandlerOccupiedException();

        //cancellation.Register(() => tcs.TrySetCanceled(cancellation));

        var received = 0;

        if (DataAvailable > 0)
            received = Receive(buffer.Span);

        if (received == buffer.Length)
            return Task.FromResult(received);

        IEnumerable<int> ContinueReceive()
        {
            do
            {
                received += Receive(buffer.Span.Slice(received));
                yield return received;
            } while (received != buffer.Length);
        }

        var continuation = ContinueReceive();
        var continuationEnumerator = continuation.GetEnumerator();

        var tcs = new TaskCompletionSource<int>();

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        void ReceiveMoreDataAsync(QuicStream sender)
        {
            if (continuationEnumerator.MoveNext() && !cancellation.IsCancellationRequested)
                return;

            tcs.TrySetResult(continuationEnumerator.Current);
            DataReceived -= ReceiveMoreDataAsync;
            continuationEnumerator.Dispose();
        }

        DataReceived += ReceiveMoreDataAsync;

        return tcs.Task;
    }
    private static InvalidOperationException GetDataReceivedHandlerOccupiedException()
        => new("Something else is already registered to receive DataReceived events (see HasDataReceivedHandler).");

    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    public unsafe int Peek(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
            return 0;

        var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
        switch (rs)
        {
            case 0: return 0;

            case -1: throw new ObjectDisposedException(nameof(QuicStream));
        }

        if (!Monitor.IsEntered(_lastRecvLock))
            throw new InvalidOperationException("Can only invoke Receive from inside a DataReceive event.");

        //lock (_lastRecvLock)
        {
            var received = 0;
            var bufferAvailable = buffer.Length;

            var lastRecvBufIndex = _lastRecvBufIndex;
            var lastRecvBufsCount = _lastRecvBufsCount;
            var lastRecvBufOffset = _lastRecvBufOffset;

            for (;;)
            {
                if (lastRecvBufIndex >= lastRecvBufsCount)
                    return received;

                var availData = _lastRecvBufs[lastRecvBufIndex].ReadOnlySpan.Slice((int)lastRecvBufOffset);
                var availDataLength = availData.Length;

                var readLength = Math.Min(availDataLength, bufferAvailable);

                availData.Slice(received, readLength).CopyTo(buffer);

                bufferAvailable -= readLength;
                received += readLength;

                if (readLength == availDataLength)
                {
                    lastRecvBufOffset = 0;
                    ++lastRecvBufIndex;

                    if (lastRecvBufIndex != lastRecvBufsCount)
                    {
                        // still more buffers to read

                        // should never actually be < 0
                        Debug.Assert(bufferAvailable >= 0);
                        if (bufferAvailable > 0)
                            continue;
                        break;
                    }
                    return received;
                }

                lastRecvBufOffset += (uint)readLength;

                // should never actually be < 0
                Debug.Assert(bufferAvailable >= 0);
                if (bufferAvailable <= 0)
                    break;
            }

            //Registration.Table.StreamReceiveComplete(_handle, (ulong)received);
            return received;
        }
    }

    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    public unsafe nuint Peek(BigSpan<byte> buffer)
    {
        if (buffer.IsEmpty)
            return 0;

        var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
        switch (rs)
        {
            case 0: return 0;

            case -1: throw new ObjectDisposedException(nameof(QuicStream));
        }

        if (!Monitor.IsEntered(_lastRecvLock))
            throw new InvalidOperationException("Can only invoke Receive from inside a DataReceive event.");

        //lock (_lastRecvLock)
        {
            nuint received = 0u;
            var bufferAvailable = buffer.Length;

            var lastRecvBufIndex = _lastRecvBufIndex;
            var lastRecvBufsCount = _lastRecvBufsCount;
            var lastRecvBufOffset = _lastRecvBufOffset;

            for (;;)
            {
                if (lastRecvBufIndex >= lastRecvBufsCount)
                    return received;

                var availData = _lastRecvBufs[lastRecvBufIndex].GetReadOnlyBigSpan().Slice(lastRecvBufOffset);
                var availDataLength = availData.Length;

                var readLength = (nuint)Math.Min(availDataLength, bufferAvailable);

                availData.Slice(received, readLength).CopyTo(buffer);

                bufferAvailable -= readLength;
                received += readLength;

                if (readLength == availDataLength)
                {
                    lastRecvBufOffset = 0;
                    ++lastRecvBufIndex;

                    if (lastRecvBufIndex != lastRecvBufsCount)
                    {
                        // still more buffers to read

                        // should never actually be < 0
                        Debug.Assert(bufferAvailable >= 0);
                        if (bufferAvailable > 0)
                            continue;
                        break;
                    }
                    return received;
                }

                lastRecvBufOffset += (uint)readLength;

                // should never actually be < 0
                Debug.Assert(bufferAvailable >= 0);
                if (bufferAvailable <= 0)
                    break;
            }

            //Registration.Table.StreamReceiveComplete(_handle, (ulong)received);
            return received;
        }
    }


    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    public unsafe int Receive(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
            return 0;

        var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
        switch (rs)
        {
            case 0: return 0;

            case -1: throw new ObjectDisposedException(nameof(QuicStream));
        }

        if (!Monitor.IsEntered(_lastRecvLock))
            throw new InvalidOperationException("Can only invoke Receive from inside a DataReceive event.");

        //lock (_lastRecvLock)
        {
            var received = 0;
            var bufferAvailable = buffer.Length;

            for (;;)
            {
                if (_lastRecvBufIndex >= _lastRecvBufsCount)
                    return received;

                var availData = _lastRecvBufs[_lastRecvBufIndex].ReadOnlySpan.Slice((int)_lastRecvBufOffset);
                var availDataLength = availData.Length;

                var readLength = Math.Min(availDataLength, bufferAvailable);

                availData.Slice(received, readLength).CopyTo(buffer);

                bufferAvailable -= readLength;
                received += readLength;
                _lastRecvTotalRead += (uint)readLength;

                if (readLength == availDataLength)
                {
                    _lastRecvBufOffset = 0;
                    ++_lastRecvBufIndex;

                    if (_lastRecvBufIndex != _lastRecvBufsCount)
                    {
                        // still more buffers to read

                        // should never actually be < 0
                        Debug.Assert(bufferAvailable >= 0);
                        if (bufferAvailable > 0)
                            continue;
                        break;
                    }

                    // all buffers were read
                    //Registration.Table.StreamReceiveComplete(_handle, (ulong)received);
                    Registration.Table.StreamReceiveComplete(_handle, _lastRecvTotalRead);
                    Registration.Table.StreamReceiveSetEnabled(_handle, 1);
                    return received;
                }

                _lastRecvBufOffset += (uint)readLength;

                // should never actually be < 0
                Debug.Assert(bufferAvailable >= 0);
                if (bufferAvailable <= 0)
                    break;
            }

            //Registration.Table.StreamReceiveComplete(_handle, (ulong)received);
            return received;
        }
    }

    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    public unsafe nuint Receive(BigSpan<byte> buffer)
    {
        if (buffer.IsEmpty)
            return 0;

        var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
        switch (rs)
        {
            case 0: return 0;

            case -1: throw new ObjectDisposedException(nameof(QuicStream));
        }

        if (!Monitor.IsEntered(_lastRecvLock))
            throw new InvalidOperationException("Can only invoke Receive from inside a DataReceive event.");

        //lock (_lastRecvLock)
        {
            nuint received = 0;
            var bufferAvailable = buffer.Length;

            for (;;)
            {
                if (_lastRecvBufIndex >= _lastRecvBufsCount)
                    return received;

                var availData = _lastRecvBufs[_lastRecvBufIndex].GetReadOnlyBigSpan().Slice(_lastRecvBufOffset);
                var availDataLength = availData.Length;

                var readLength = availDataLength > bufferAvailable ? bufferAvailable : availDataLength; // Math.Min

                availData.Slice(received, readLength).CopyTo(buffer);

                bufferAvailable -= readLength;
                received += readLength;
                _lastRecvTotalRead += (uint)readLength;

                if (readLength == availDataLength)
                {
                    _lastRecvBufOffset = 0;
                    ++_lastRecvBufIndex;

                    if (_lastRecvBufIndex != _lastRecvBufsCount)
                    {
                        // still more buffers to read

                        // should never actually be < 0
                        Debug.Assert(bufferAvailable >= 0);
                        if (bufferAvailable > 0)
                            continue;
                        break;
                    }

                    // all buffers were read
                    //Registration.Table.StreamReceiveComplete(_handle, (ulong)received);
                    Registration.Table.StreamReceiveComplete(_handle, _lastRecvTotalRead);
                    Registration.Table.StreamReceiveSetEnabled(_handle, 1);
                    return received;
                }

                _lastRecvBufOffset += (uint)readLength;

                // should never actually be < 0
                Debug.Assert(bufferAvailable >= 0);
                if (bufferAvailable <= 0)
                    break;
            }

            //Registration.Table.StreamReceiveComplete(_handle, (ulong)received);
            return received;
        }
    }

    [SuppressMessage("Reliability", "CA2000", Justification = "Disposed in callback")]
    public unsafe Task SendAsync(ReadOnlyMemory<byte> data, QUIC_SEND_FLAGS flags = QUIC_SEND_FLAGS.QUIC_SEND_FLAG_NONE)
    {
        CheckDisposed();
        if (data.IsEmpty) throw new ArgumentException("Should not be empty.", nameof(data));

        var h = data.Pin();

        var buf = NativeMemory.New<QUIC_BUFFER>();
        if (buf == null) throw new NotImplementedException("Failed to allocate native memory.");
        buf->Buffer = (byte*)h.Pointer;
        buf->Length = (uint)data.Length;

        var tcs = new TaskCompletionSource<bool>();

        var hCtx = new GcHandle<PinnedDataSendContext>(new(h, buf, tcs));

        CheckDisposed();
        Registration.Table.StreamSend(Handle, buf, 1, flags, (void*)GCHandle.ToIntPtr(hCtx));

        return tcs.Task;
    }

    public Task SendAsync(params ReadOnlyMemory<byte>[] data)
        => SendAsync(QUIC_SEND_FLAGS.QUIC_SEND_FLAG_NONE, data);

    public Task SendAsync(ReadOnlyMemory<byte>[] data, QUIC_SEND_FLAGS flags)
        => SendAsync(flags, data);

    [SuppressMessage("Reliability", "CA2000", Justification = "Disposed in callback")]
    public unsafe Task SendAsync(QUIC_SEND_FLAGS flags, params ReadOnlyMemory<byte>[] data)
    {
        CheckDisposed();
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) throw new ArgumentException("Array must not be empty.", nameof(data));

        var l = (uint)data.Length;
        var buf = NativeMemory.New<QUIC_BUFFER>(l);
        if (buf == null) throw new NotImplementedException("Failed to allocate native memory.");
        var pins = new MemoryHandle[l];
        for (var i = 0; i < l; i++)
        {
            var m = data[i];
            var h = m.Pin();
            pins[i] = h;

            buf[i].Buffer = (byte*)h.Pointer;
            buf[i].Length = (uint)m.Length;
        }

        var tcs = new TaskCompletionSource<bool>();

        var hCtx = new GcHandle<PinnedMultipleDataSendContext>(new(pins, buf, tcs));

        CheckDisposed();
        Registration.Table.StreamSend(Handle, buf, l, flags, (void*)GCHandle.ToIntPtr(hCtx));

        return tcs.Task;
    }

    [SuppressMessage("Reliability", "CA2000", Justification = "Disposed in callback")]
    public unsafe Task SendAsync(QUIC_BUFFER* buf, uint count, QUIC_SEND_FLAGS flags = QUIC_SEND_FLAGS.QUIC_SEND_FLAG_NONE)
    {
        CheckDisposed();
        if (buf == null) throw new ArgumentNullException(nameof(buf));
        if (count == 0) throw new ArgumentOutOfRangeException(nameof(count), "Should be greater than zero.");

        var tcs = new TaskCompletionSource<bool>();

        var hCtx = new GcHandle<BufferSendContext>(new(buf, tcs));

        CheckDisposed();
        Registration.Table.StreamSend(Handle, buf, count, flags, (void*)GCHandle.ToIntPtr(hCtx));

        return tcs.Task;
    }

    public unsafe void Start()
    {
        if (IsStarted) return;

        var status = Registration.Table.StreamStart(Handle,
            QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_IMMEDIATE
            | QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_SHUTDOWN_ON_FAIL);
        AssertNotFailure(status);
        if (status == QUIC_STATUS_PENDING)
        {
            Interlocked.Exchange(ref _runState, 1);
            _started = false;
        }
        else
        {
            Interlocked.Exchange(ref _runState, 2);
            _started = true;
            OnStartComplete(status);
        }
    }

    public EventHandler<QuicStream, int>? StartComplete;

    private void OnStartComplete(int status)
        => StartComplete?.Invoke(this, status);

    public Task<int> WaitForStartCompleteAsync(CancellationToken ct = default)
    {
        static void Cancellation(object? o)
        {
            var (tcs, ct) = ((TaskCompletionSource<int>, CancellationToken))o!;
            tcs.TrySetCanceled(ct);
        }

        var tcs = new TaskCompletionSource<int>();

        void Completion(QuicStream stream, int status)
        {
            tcs.TrySetResult(status);
            stream.StartComplete -= Completion;
        }

        ct.Register(Cancellation, (tcs, ct));

        StartComplete += Completion;

        return tcs.Task;
    }

    public unsafe void Shutdown()
    {
        if (Registration.Disposed) return;

        // TODO: validate _runState?
        Registration.Table.StreamShutdown(Handle,
            QUIC_STREAM_SHUTDOWN_FLAGS.QUIC_STREAM_SHUTDOWN_FLAG_IMMEDIATE
            | QUIC_STREAM_SHUTDOWN_FLAGS.QUIC_STREAM_SHUTDOWN_FLAG_ABORT, 0);
    }

    internal unsafe void Reject()
    {
        if (Registration.Disposed) return;

        // TODO: validate _runState?
        Connection.Shutdown();
        Registration.Table.SetCallbackHandler(Handle, null, null);
        //Registration.Table.StreamClose(Handle);
        _gcHandle.Free();
    }


    public unsafe void Close()
    {
        if (Registration.Disposed) return;

        // TODO: validate _runState?
        Connection.Shutdown();
        //Registration.Table.SetCallbackHandler(Handle, null, null);
        if (!Registration.Disposed)
            Registration.Table.StreamClose(Handle);
        _gcHandle.Free();
    }

    public void Dispose()
    {
        var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
        switch (rs)
        {
            case < 0:
                return;

            case 0:
                Reject();
                break;

            default:
                Close();
                break;
        }
    }

    public bool Disposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Interlocked.CompareExchange(ref _runState, 0, 0) < 0
            || Registration.Disposed;
    }

    private void CheckDisposed()
    {
        if (Disposed) throw new ObjectDisposedException(nameof(QuicStream));
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private Utilities.EventHandler<QuicStream>? _dataReceived;

    public Utilities.EventHandler<QuicStream>? DataReceived
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Interlocked.CompareExchange(ref _dataReceived, null, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            // always permit unsetting
            if (value is null)
                Interlocked.Exchange(ref _dataReceived, null);
            // don't permit resetting
            else if (Interlocked.CompareExchange(ref _dataReceived, value, null) is not null)
                throw GetDataReceivedHandlerOccupiedException();
        }
    }

    public bool HasDataReceivedHandler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DataReceived is not null;
    }

    public QUIC_RECEIVE_FLAGS LastReceiveFlags => _lastRecvFlags;

    public bool IsStarted => _started;

    private void OnDataReceived()
        => DataReceived?.Invoke(this);

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicStream, ExceptionDispatchInfo>? UnobservedException;

    private void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{LogTimeStamp.ElapsedSeconds:F6} {this} {arg!.SourceException}");
        UnobservedException?.Invoke(this, arg);
    }

    public override unsafe string ToString()
        => TryGetId(out var id)
            ? Name is null
                ? $"[QuicStream 0x{(ulong)_handle:X}]"
                : $"[QuicStream \"{Name}\"] 0x{(ulong)_handle:X}"
            : Name is null
                ? $"[QuicStream #{id} 0x{(ulong)_handle:X}]"
                : $"[QuicStream \"{Name}\" #{id}] 0x{(ulong)_handle:X}";
}
