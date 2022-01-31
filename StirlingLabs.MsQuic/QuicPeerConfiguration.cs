using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Quic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
[SuppressMessage("Design", "CA1063", Justification = "It's fine")]
public abstract class QuicPeerConfiguration : IDisposable
{
    public const QUIC_CREDENTIAL_FLAGS DefaultQuicCredentialFlags
        = QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_USE_TLS_BUILTIN_CERTIFICATE_VALIDATION
        | QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_INDICATE_CERTIFICATE_RECEIVED
        | QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_DEFER_CERTIFICATE_VALIDATION
        | QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_USE_PORTABLE_CERTIFICATES
        | QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_SET_ALLOWED_CIPHER_SUITES;

    public const QUIC_ALLOWED_CIPHER_SUITE_FLAGS DefaultAllowedCipherSuites
        = QUIC_ALLOWED_CIPHER_SUITE_FLAGS.QUIC_ALLOWED_CIPHER_SUITE_AES_128_GCM_SHA256
        | QUIC_ALLOWED_CIPHER_SUITE_FLAGS.QUIC_ALLOWED_CIPHER_SUITE_AES_256_GCM_SHA384
        | QUIC_ALLOWED_CIPHER_SUITE_FLAGS.QUIC_ALLOWED_CIPHER_SUITE_CHACHA20_POLY1305_SHA256;
    public QuicRegistration Registration { get; }

    public unsafe QUIC_HANDLE* Handle { get; protected set; }

    public bool CredentialsConfigured { get; protected set; }

    protected QuicPeerConfiguration(QuicRegistration registration)
        => Registration = registration ?? throw new ArgumentNullException(nameof(registration));

    [SuppressMessage("Design", "CA1063", Justification = "Annoying")]
    public unsafe void Dispose()
    {
        if (!Registration.Disposed)
            Registration.Table.ConfigurationClose(Handle);
        Handle = null;
        GC.SuppressFinalize(this);
    }
}
