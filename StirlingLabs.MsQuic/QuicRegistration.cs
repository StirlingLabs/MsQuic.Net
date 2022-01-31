using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;
using static Microsoft.Quic.MsQuic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed unsafe class QuicRegistration : IDisposable
{
    static QuicRegistration()
        => Init();

    private readonly QUIC_API_TABLE* _table;
    private QUIC_HANDLE* _handle;


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
        get => _handle == null;
    }

    public ref QUIC_API_TABLE Table
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (Disposed)
                throw new ObjectDisposedException(nameof(QuicRegistration));
            return ref *_table;
        }
    }

    public QUIC_HANDLE* Handle
    {
        get => Disposed ? throw new ObjectDisposedException(nameof(QuicRegistration)) : _handle;
        private set => _handle = value;
    }

    public void Shutdown(ulong code)
        => Table.RegistrationShutdown(Handle, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, code);

    public void Dispose()
    {
        if (Disposed)
            return;

        _table->RegistrationClose(_handle);

        Close(_table);

        _handle = null;

    }
}
