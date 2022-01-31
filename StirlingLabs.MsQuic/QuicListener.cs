using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;
using static Microsoft.Quic.MsQuic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicListener : IDisposable
{
    public QuicServerConfiguration Configuration { get; }
    public unsafe QUIC_HANDLE* Handle { get; private set; }
    private GcHandle<QuicListener> _gcHandle;
    public QuicRegistration Registration => Configuration.Registration;

    public unsafe QuicListener(QuicServerConfiguration config)
    {
        Configuration = config;

        Handle = null;

        _gcHandle = new(this);

        var p = (void*)GCHandle.ToIntPtr(_gcHandle);

        QUIC_HANDLE* handle = null;

        try
        {
#if NET5_0_OR_GREATER
            var status = Registration.Table.ListenerOpen(Registration.Handle, &NativeListenerOpenCallback, p, &handle);
#else
            var status = Registration.Table.ListenerOpen(Registration.Handle, NativeListenerOpenCallbackThunkPointer, p, &handle);
#endif

            AssertSuccess(status);

            Handle = handle;
        }
        catch
        {
            _gcHandle.Free();
            throw;
        }
    }


    private ConcurrentDictionary<QuicPeerConnection, Nothing?> _connections = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AddConnection(QuicPeerConnection connection)
        => Interlocked.CompareExchange(ref _runState, 0, 0) != 0
            && _connections.TryAdd(connection, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool RemoveConnection(QuicPeerConnection connection)
        => _connections.TryRemove(connection, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearConnections()
        => _connections.Clear();

#if NET5_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    private static unsafe int NativeListenerOpenCallback(QUIC_HANDLE* handle, void* context, QUIC_LISTENER_EVENT* @event)
    {
        var listener = (QuicListener)GCHandle.FromIntPtr((IntPtr)context).Target!;
        return listener.ManagedListenerOpenCallback(ref *@event);
    }

#if !NET5_0_OR_GREATER
    private unsafe delegate int NativeListenerOpenCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_LISTENER_EVENT* @event);

    private static readonly unsafe NativeListenerOpenCallbackDelegate NativeListenerOpenCallbackThunk = NativeListenerOpenCallback;
    private static readonly unsafe delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_LISTENER_EVENT*, int>
        NativeListenerOpenCallbackThunkPointer
            = (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_LISTENER_EVENT*, int>)Marshal.GetFunctionPointerForDelegate(
                NativeListenerOpenCallbackThunk);
#endif

    [SuppressMessage("Design", "CA1031", Justification = "Execution completion critical, native event handler callback")]
    [SuppressMessage("Reliability", "CA2000", Justification = "Connection objects are collected and disposed out of scope")]
    private unsafe int ManagedListenerOpenCallback(ref QUIC_LISTENER_EVENT @event)
    {
        switch (@event.Type)
        {
            case QUIC_LISTENER_EVENT_TYPE.QUIC_LISTENER_EVENT_NEW_CONNECTION:
                ref var typedEvent = ref @event.NEW_CONNECTION;
                var connection = new QuicServerConnection(Configuration, typedEvent.Connection, typedEvent.Info);
                var status = Registration.Table.ConnectionSetConfiguration(typedEvent.Connection, Configuration.Handle);
                try
                {
                    if (status != QUIC_STATUS_PENDING)
                        AssertSuccess(status);
                }
                catch (Exception ex)
                {
                    connection.Close();
                    OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
                    break;
                }
                AddConnection(connection);
                OnNewConnection(connection);
                if (IsPending(status))
                    connection.Connected += OnClientConnected;
                else
                    OnClientConnected(connection);
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type}");
                break;
            default:
                Trace.TraceInformation($"{TimeStamp.Elapsed} {this} {@event.Type} Unhandled");
                break;
        }
        return 0;
    }

    public unsafe void Dispose()
    {
        if (Registration.Disposed)
            Interlocked.Exchange(ref _runState, 0);
        else
        {
            Stop();
            Registration.Table.ListenerClose(Handle);
        }
        Handle = null;

        if (_gcHandle != default)
            _gcHandle.Free();
    }


    private int _runState;

    public unsafe void Start(IPEndPoint endPoint)
    {
        if (!Configuration.CredentialsConfigured)
            throw new InvalidOperationException("Credentials must be configured before starting the listener.");

        if (Interlocked.CompareExchange(ref _runState, 1, 0) != 0) return;

        var alpnCount = Configuration.Alpns.Length;

        Span<QUIC_BUFFER> quicAlpns = stackalloc QUIC_BUFFER[alpnCount];

        for (var i = 0; i < alpnCount; ++i)
        {
            ref var quicAlpn = ref quicAlpns[i];
            ref var alpn = ref Configuration.Alpns[i];
            quicAlpn.Buffer = (byte*)alpn.Pointer;
            quicAlpn.Length = (uint)alpn.Length;
        }

        var sa = sockaddr.New(endPoint);

        try
        {
            fixed (QUIC_BUFFER* pQuicAlpns = quicAlpns)
            {
                AssertSuccess(
                    Registration.Table.ListenerStart(Handle, pQuicAlpns, 1, sa));
            }

            Interlocked.Exchange(ref _runState, 2);
        }
        finally
        {
            sockaddr.Free(sa);
        }
    }


    public unsafe void Stop()
    {
        var runState = Interlocked.CompareExchange(ref _runState, 0, 0);

        switch (runState)
        {
            case 0:
                return;
            case 1:
                throw new InvalidOperationException("QuicListener can not stop while still starting. Wait for startup to complete first.");
            case 2:
                Registration.Table.ListenerStop(Handle);
                Interlocked.Exchange(ref _runState, 0);
                foreach (var connection in _connections.Keys)
                    connection.Dispose();
                return;
            default:
                throw new NotImplementedException();
        }
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicListener, QuicServerConnection>? NewConnection;

    private void OnNewConnection(QuicServerConnection connection)
    {
        Trace.TraceInformation($"{TimeStamp.Elapsed} {this} event NewConnection {connection}");
        NewConnection?.Invoke(this, connection);
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicListener, QuicServerConnection>? ClientConnected;

    private void OnClientConnected(QuicServerConnection connection)
    {
        Trace.TraceInformation($"{TimeStamp.Elapsed} {this} event ClientConnected {connection}");
        ClientConnected?.Invoke(this, connection);
        connection.Connected -= OnClientConnected;
    }

    public event EventHandler<QuicListener, ExceptionDispatchInfo>? UnobservedException;

    private void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{TimeStamp.Elapsed} {this} {arg.SourceException}");
        UnobservedException?.Invoke(this, arg);
    }
}
