using System;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using StirlingLabs.MsQuic.Bindings;
using StirlingLabs.Utilities;
using static StirlingLabs.MsQuic.Bindings.MsQuic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed unsafe class QuicRegistration : IDisposable
{
    static QuicRegistration()
        => Init();

    private readonly QUIC_API_TABLE* _table;
    private nint _handle;


    public QuicRegistration(string name) : this(SizedUtf8String.Create(name)) { }

    public QuicRegistration(SizedUtf8String name)
    {
        _table = Open();
        QUIC_HANDLE* handle = null;
        QUIC_REGISTRATION_CONFIG config;
        config.ExecutionProfile = QUIC_EXECUTION_PROFILE.QUIC_EXECUTION_PROFILE_LOW_LATENCY;
        config.AppName = name.Pointer;
        var status = _table->RegistrationOpen(&config, &handle);
        if (IsFailure(status))
        {
            Close(_table);
            _table = null;
            AssertNotFailure(status);
        }
        Handle = handle;
    }

    public bool Disposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Interlocked.CompareExchange(ref _handle, (nint)0, (nint)0) is (nint)0;
    }

    public ref QUIC_API_TABLE Table
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (Disposed) throw new ObjectDisposedException(nameof(QuicRegistration));
            return ref *_table;
        }
    }

    public QUIC_HANDLE* Handle
    {
        get {
            var v = Interlocked.CompareExchange(ref _handle, (nint)0, (nint)0);
            if (v is (nint)0) throw new ObjectDisposedException(nameof(QuicRegistration));
            return (QUIC_HANDLE*)v;
        }
        private set => Interlocked.Exchange(ref _handle, (nint)value);
    }

    public void Shutdown(ulong code)
        => Table.RegistrationShutdown(Handle, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, code);

    public void Dispose()
    {
        if (Disposed)
            return;

        _table->RegistrationClose(Handle);

        Close(_table);

        Handle = null;

    }
}
