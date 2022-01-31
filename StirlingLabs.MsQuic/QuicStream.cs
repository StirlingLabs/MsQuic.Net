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

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed partial class QuicStream : IDisposable
{
    public QuicRegistration Registration { get; }

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

    public unsafe QuicStream(QuicRegistration registration, QuicPeerConnection connection)
    {
        _gcHandle = GCHandle.Alloc(this);
        Registration = registration;
        Connection = connection;

        QUIC_HANDLE* handle = null;
        try
        {
            var status = Registration.Table.StreamOpen(Connection.Handle,
                QUIC_STREAM_OPEN_FLAGS.QUIC_STREAM_OPEN_FLAG_0_RTT,
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

    public ulong IdealSendBufferSize { get; private set; }


    private unsafe uint DataAvailable
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
        Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type}");

        switch (@event.Type)
        {
            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_RECEIVE: {
                ref var typedEvent = ref @event.RECEIVE;

                Debug.Assert(_lastRecvBufIndex >= _lastRecvBufsCount);
                lock (_lastRecvLock)
                {
                    _lastRecvFlags = typedEvent.Flags;
                    _lastRecvBufs = typedEvent.Buffers;
                    _lastRecvBufsCount = typedEvent.BufferCount;
                    _lastRecvBufIndex = 0;
                    _lastRecvBufOffset = 0;
                    _lastRecvTotalRead = 0;
                    Interlocked.Exchange(ref _runState, 1);
                    OnDataReceived();
                }
                return DataAvailable == 0
                    ? QUIC_STATUS_SUCCESS
                    : QUIC_STATUS_PENDING;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_START_COMPLETE: {
                ref var typedEvent = ref @event.START_COMPLETE;
                if (IsSuccess(typedEvent.Status))
                    Interlocked.Exchange(ref _runState, 1);
                else
                    Close();
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_SEND_COMPLETE: {
                var pClientCtx = (IntPtr)@event.SEND_COMPLETE.ClientContext;
                var gchClientCtx = GCHandle.FromIntPtr(pClientCtx);
                var disposable = (IDisposable?)gchClientCtx.Target;
                if (disposable is SendContext sc)
                    sc.TaskCompletionSource.TrySetResult(true);
                disposable?.Dispose();
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_IDEAL_SEND_BUFFER_SIZE: {
                ref var typedEvent = ref @event.IDEAL_SEND_BUFFER_SIZE;
                IdealSendBufferSize = typedEvent.ByteCount;
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
                return 0;
            }

            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED: {
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
                return 0;
            }
            case QUIC_STREAM_EVENT_TYPE.QUIC_STREAM_EVENT_PEER_SEND_SHUTDOWN: {
                Interlocked.Exchange(ref _runState, -1);
                return 0;
            }
            default: throw new NotImplementedException(@event.Type.ToString());
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
        if (Started) return;
        Registration.Table.StreamStart(Handle,
            QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_ASYNC
            | QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_IMMEDIATE
            | QUIC_STREAM_START_FLAGS.QUIC_STREAM_START_FLAG_SHUTDOWN_ON_FAIL);
        _started = true;
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
        // TODO: validate _runState?
        Connection.Shutdown();
        Registration.Table.SetCallbackHandler(Handle, null, null);
        //Registration.Table.StreamClose(Handle);
        _gcHandle.Free();
    }


    public unsafe void Close()
    {
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
    public event EventHandler<QuicStream>? DataReceived
    {
        add {
            if (HasDataReceivedHandler)
                throw GetDataReceivedHandlerOccupiedException();
            _dataReceived += value;
        }
        remove => _dataReceived -= value;
    }

    private event EventHandler<QuicStream>? _dataReceived;

    public bool HasDataReceivedHandler => _dataReceived is not null;

    public QUIC_RECEIVE_FLAGS LastReceiveFlags => _lastRecvFlags;

    public bool Started => _started;

    private void OnDataReceived()
        => _dataReceived?.Invoke(this);

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicStream, ExceptionDispatchInfo>? UnobservedException;

    private void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{TimeStamp.Elapsed} {this} {arg.SourceException}");
        UnobservedException?.Invoke(this, arg);
    }

    public override unsafe string ToString()
        => $"[QuicStream 0x{(ulong)_handle:X}]";
}
