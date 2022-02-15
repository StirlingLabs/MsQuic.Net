using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Native;
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

    private Memory<byte> _unreadBuffer;
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

        Interlocked.Exchange(ref _runState, -1);

        registration.Table.SetCallbackHandler(handle, nativeCallback, (void*)(IntPtr)_gcHandle);
        _started = true;
    }

    internal unsafe QuicStream(QuicRegistration registration, QuicPeerConnection connection, QUIC_HANDLE* handle, QUIC_STREAM_OPEN_FLAGS flags,
        long id)
        : this(registration, connection, handle, flags)
        => Registration.Table.SetParam(Handle, QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_STREAM, QUIC_PARAM_STREAM_ID, 8, &id);

    public string? Name { get; set; }

    public QuicRegistration Registration { get; }

    public unsafe long Id
    {
        get {

            var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
            if (rs <= 0) throw new InvalidOperationException("Stream is not initialized.");
            uint l = sizeof(long);
            long value = -1;
            var status = Registration.Table.GetParam(Handle, QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_STREAM, QUIC_PARAM_STREAM_ID, &l, &value);
            AssertSuccess(status);
            return value;
        }
        //set => Registration.Table.SetParam(Handle, QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_STREAM, QUIC_PARAM_STREAM_ID, 8, &value);
    }

    public unsafe bool TryGetId(out long id)
    {
        uint l = sizeof(long);
        fixed (long* pId = &id)
            return IsSuccess(Registration.Table.GetParam(Handle, QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_STREAM, QUIC_PARAM_STREAM_ID, &l, pId));
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
                    return (uint)_unreadBuffer.Length;
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
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED: {
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_SHUTDOWN_COMPLETE: {
                ref var typedEvent = ref @event.SHUTDOWN_COMPLETE;
                // TODO: ???
                if (typedEvent.ConnectionShutdown == 0)
                {
                    if (_unreadBuffer.IsEmpty)
                    {
                        var avail = DataAvailable;
                        if (avail > 0)
                        {
                            Debug.Assert(_unreadBuffer.IsEmpty);
                            var buffer = new Memory<byte>(new byte[avail]);
                            // save the unread memory
                            Receive(buffer.Span);
                            _unreadBuffer = buffer;
                        }
                    }
                    Interlocked.Exchange(ref _runState, -1);
                }
                else
                    Interlocked.Exchange(ref _runState, -2);
                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{ConnectionShutdown={typedEvent.ConnectionShutdown}}}");
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
    public unsafe int Receive(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
            return 0;

        var rs = Interlocked.CompareExchange(ref _runState, 0, 0);
        switch (rs)
        {
            case 0: return 0;

            case -1: {
                // stream was finalized, can read any saved unread

                if (_unreadBuffer.IsEmpty)
                    return 0;

                var copied = Math.Min(_unreadBuffer.Length, buffer.Length);
                _unreadBuffer.Span.CopyTo(buffer);
                _unreadBuffer = copied != _unreadBuffer.Length
                    ? _unreadBuffer.Slice(copied)
                    : Memory<byte>.Empty;
                return copied;
            }
        }

        lock (_lastRecvLock)
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
                    Registration.Table.StreamReceiveSetEnabled(_handle, true);
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
    public unsafe Task SendAsync(Memory<byte> data, QUIC_SEND_FLAGS flags = QUIC_SEND_FLAGS.QUIC_SEND_FLAG_NONE)
    {
        var h = data.Pin();

        var buf = NativeMemory.New<QUIC_BUFFER>();
        buf->Buffer = (byte*)h.Pointer;
        buf->Length = (uint)data.Length;

        var tcs = new TaskCompletionSource<bool>();

        var hCtx = new GcHandle<PinnedDataSendContext>(new(h, buf, tcs));

        Registration.Table.StreamSend(Handle, buf, 1, flags, (void*)GCHandle.ToIntPtr(hCtx));

        return tcs.Task;
    }

    [SuppressMessage("Reliability", "CA2000", Justification = "Disposed in callback")]
    public unsafe Task SendAsync(QUIC_BUFFER* buf, QUIC_SEND_FLAGS flags = QUIC_SEND_FLAGS.QUIC_SEND_FLAG_NONE)
    {
        var tcs = new TaskCompletionSource<bool>();

        var hCtx = new GcHandle<BufferSendContext>(new(buf, tcs));

        Registration.Table.StreamSend(Handle, buf, 0, flags, (void*)GCHandle.ToIntPtr(hCtx));

        return tcs.Task;
    }

    public unsafe void Start()
    {
        if (IsStarted) return;

        var status = Registration.Table.StreamStart(Handle,
            QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_ASYNC
            | QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_IMMEDIATE
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
        if (Interlocked.CompareExchange(ref _runState, 0, 0) == 0)
            Reject();
        else
            Close();
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event Utilities.EventHandler<QuicStream>? DataReceived
    {
        add {
            if (HasDataReceivedHandler)
                throw GetDataReceivedHandlerOccupiedException();
            _dataReceived += value;
        }
        remove => _dataReceived -= value;
    }

    private event Utilities.EventHandler<QuicStream>? _dataReceived;

    public bool HasDataReceivedHandler => _dataReceived is not null;

    public QUIC_RECEIVE_FLAGS LastReceiveFlags => _lastRecvFlags;

    public bool IsStarted => _started;

    private void OnDataReceived()
        => _dataReceived?.Invoke(this);

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicStream, ExceptionDispatchInfo>? UnobservedException;

    private void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{LogTimeStamp.ElapsedSeconds:F6} {this} {arg.SourceException}");
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
