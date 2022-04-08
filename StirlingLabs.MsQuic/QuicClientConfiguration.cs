using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;
using static Microsoft.Quic.MsQuic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicClientConfiguration : QuicPeerConfiguration
{
    public readonly SizedUtf8String[] Alpns;

    public QuicClientConfiguration(QuicRegistration registration, bool reliableDatagrams, params SizedUtf8String[] alpns)
        : this(registration, alpns)
        => DatagramsAreReliable = reliableDatagrams;

    public unsafe QuicClientConfiguration(QuicRegistration registration, params SizedUtf8String[] alpns)
        : base(registration)
    {
        if (registration is null)
            throw new ArgumentNullException(nameof(registration));

        Alpns = alpns ?? throw new ArgumentNullException(nameof(alpns));

        var alpnCount = alpns.Length;

        Span<QUIC_BUFFER> quicAlpns = stackalloc QUIC_BUFFER[alpnCount];

        for (var i = 0; i < alpnCount; ++i)
        {
            ref var quicAlpn = ref quicAlpns[i];
            ref var alpn = ref alpns[i];
            quicAlpn.Buffer = (byte*)alpn.Pointer;
            quicAlpn.Length = (uint)alpn.Length;
        }

        var settings = new QUIC_SETTINGS
        {
            PeerUnidiStreamCount = ushort.MaxValue,
            PeerBidiStreamCount = ushort.MaxValue,
            SendBufferingEnabled = 0,
            DatagramReceiveEnabled = 1,
            ServerResumptionLevel = (byte)QUIC_SERVER_RESUMPTION_LEVEL.QUIC_SERVER_RESUME_AND_ZERORTT,
            KeepAliveIntervalMs = 5000,
            IsSet = new()
            {
                PeerUnidiStreamCount = 1,
                PeerBidiStreamCount = 1,
                SendBufferingEnabled = 1,
                DatagramReceiveEnabled = 1,
                ServerResumptionLevel = 1,
                KeepAliveIntervalMs = 1,
            }
        };

        QUIC_HANDLE* handle = null;

        fixed (QUIC_BUFFER* pAlpns = quicAlpns)
        {
            AssertSuccess(registration.Table
                .ConfigurationOpen(registration.Handle,
                    pAlpns, (uint)alpnCount, &settings, (uint)sizeof(QUIC_SETTINGS),
                    null, &handle));
        }

        Handle = handle;
    }

    public unsafe QuicClientConfiguration(QuicRegistration registration, QUIC_SETTINGS* pSettings, params SizedUtf8String[] alpns)
        : base(registration)
    {
        if (registration is null)
            throw new ArgumentNullException(nameof(registration));

        Alpns = alpns ?? throw new ArgumentNullException(nameof(alpns));

        var alpnCount = alpns.Length;

        Span<QUIC_BUFFER> quicAlpns = stackalloc QUIC_BUFFER[alpnCount];

        for (var i = 0; i < alpnCount; ++i)
        {
            ref var quicAlpn = ref quicAlpns[i];
            ref var alpn = ref alpns[i];
            quicAlpn.Buffer = (byte*)alpn.Pointer;
            quicAlpn.Length = (uint)alpn.Length;
        }

        QUIC_HANDLE* handle = null;

        fixed (QUIC_BUFFER* pAlpns = quicAlpns)
        {
            AssertSuccess(registration.Table
                .ConfigurationOpen(registration.Handle,
                    pAlpns, (uint)alpnCount, pSettings, (uint)sizeof(QUIC_SETTINGS),
                    null, &handle));
        }
        Handle = handle;
    }

    public unsafe void ConfigureCredentials(in QuicCertificate quicCert,
        QUIC_CREDENTIAL_FLAGS credentialFlags = DefaultQuicCredentialFlags,
        QUIC_ALLOWED_CIPHER_SUITE_FLAGS allowedCipherSuiteFlags = DefaultAllowedCipherSuites)
        => ConfigureCredentials(
            (QuicCertificate*)Unsafe.AsPointer(ref Unsafe.AsRef(quicCert)),
            credentialFlags,
            allowedCipherSuiteFlags);

    public unsafe void ConfigureCredentials(QuicCertificate* quicCert,
        QUIC_CREDENTIAL_FLAGS credentialFlags = DefaultQuicCredentialFlags,
        QUIC_ALLOWED_CIPHER_SUITE_FLAGS allowedCipherSuiteFlags = DefaultAllowedCipherSuites)
    {
        credentialFlags |= QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_CLIENT;

        if (allowedCipherSuiteFlags != 0)
            credentialFlags |= QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_SET_ALLOWED_CIPHER_SUITES;

        var credConfig = new QUIC_CREDENTIAL_CONFIG
        {
            Type = QUIC_CREDENTIAL_TYPE.QUIC_CREDENTIAL_TYPE_CERTIFICATE_PKCS12,
            CertificatePkcs12 = &quicCert->Pkcs12,
            Flags = credentialFlags,
            AllowedCipherSuites = allowedCipherSuiteFlags
        };

        AssertSuccess(Registration.Table.ConfigurationLoadCredential(Handle, &credConfig));

        CredentialsConfigured = true;
    }

    public unsafe void ConfigureCredentials(
        QUIC_CREDENTIAL_FLAGS credentialFlags = DefaultQuicCredentialFlags,
        QUIC_ALLOWED_CIPHER_SUITE_FLAGS allowedCipherSuiteFlags = DefaultAllowedCipherSuites)
    {
        credentialFlags |= QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_CLIENT;

        if (allowedCipherSuiteFlags != 0)
            credentialFlags |= QUIC_CREDENTIAL_FLAGS.QUIC_CREDENTIAL_FLAG_SET_ALLOWED_CIPHER_SUITES;

        var credConfig = new QUIC_CREDENTIAL_CONFIG
        {
            Type = QUIC_CREDENTIAL_TYPE.QUIC_CREDENTIAL_TYPE_NONE,
            Flags = credentialFlags,
            AllowedCipherSuites = allowedCipherSuiteFlags
        };

        AssertSuccess(Registration.Table.ConfigurationLoadCredential(Handle, &credConfig));

        CredentialsConfigured = true;
    }

    public bool DatagramsAreReliable { get; }
}
