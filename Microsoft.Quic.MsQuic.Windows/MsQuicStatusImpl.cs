using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.Quic;

using static QUIC_STATUS;

[PublicAPI]
public sealed class MsQuicStatusImpl : MsQuicStatusBase
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string? GetNameForStatus(int status)
        => status switch
        {
            0 => "QUIC_STATUS_SUCCESS",
            (int)QUIC_STATUS_PENDING => nameof(QUIC_STATUS_PENDING),
            (int)QUIC_STATUS_CONTINUE => nameof(QUIC_STATUS_CONTINUE),
            (int)QUIC_STATUS_OUT_OF_MEMORY => nameof(QUIC_STATUS_OUT_OF_MEMORY),
            (int)QUIC_STATUS_INVALID_PARAMETER => nameof(QUIC_STATUS_INVALID_PARAMETER),
            (int)QUIC_STATUS_INVALID_STATE => nameof(QUIC_STATUS_INVALID_STATE),
            (int)QUIC_STATUS_NOT_SUPPORTED => nameof(QUIC_STATUS_NOT_SUPPORTED),
            (int)QUIC_STATUS_NOT_FOUND => nameof(QUIC_STATUS_NOT_FOUND),
            (int)QUIC_STATUS_BUFFER_TOO_SMALL => nameof(QUIC_STATUS_BUFFER_TOO_SMALL),
            (int)QUIC_STATUS_HANDSHAKE_FAILURE => nameof(QUIC_STATUS_HANDSHAKE_FAILURE),
            (int)QUIC_STATUS_ABORTED => nameof(QUIC_STATUS_ABORTED),
            (int)QUIC_STATUS_ADDRESS_IN_USE => nameof(QUIC_STATUS_ADDRESS_IN_USE),
            (int)QUIC_STATUS_CONNECTION_TIMEOUT => nameof(QUIC_STATUS_CONNECTION_TIMEOUT),
            (int)QUIC_STATUS_CONNECTION_IDLE => nameof(QUIC_STATUS_CONNECTION_IDLE),
            (int)QUIC_STATUS_UNREACHABLE => nameof(QUIC_STATUS_UNREACHABLE),
            (int)QUIC_STATUS_INTERNAL_ERROR => nameof(QUIC_STATUS_INTERNAL_ERROR),
            (int)QUIC_STATUS_CONNECTION_REFUSED => nameof(QUIC_STATUS_CONNECTION_REFUSED),
            (int)QUIC_STATUS_PROTOCOL_ERROR => nameof(QUIC_STATUS_PROTOCOL_ERROR),
            (int)QUIC_STATUS_VER_NEG_ERROR => nameof(QUIC_STATUS_VER_NEG_ERROR),
            (int)QUIC_STATUS_TLS_ERROR => nameof(QUIC_STATUS_TLS_ERROR),
            (int)QUIC_STATUS_USER_CANCELED => nameof(QUIC_STATUS_USER_CANCELED),
            (int)QUIC_STATUS_ALPN_NEG_FAILURE => nameof(QUIC_STATUS_ALPN_NEG_FAILURE),
            (int)QUIC_STATUS_STREAM_LIMIT_REACHED => nameof(QUIC_STATUS_STREAM_LIMIT_REACHED),
            (int)QUIC_STATUS_CLOSE_NOTIFY => nameof(QUIC_STATUS_CLOSE_NOTIFY),
            (int)QUIC_STATUS_BAD_CERTIFICATE => nameof(QUIC_STATUS_BAD_CERTIFICATE),
            (int)QUIC_STATUS_UNSUPPORTED_CERTIFICATE => nameof(QUIC_STATUS_UNSUPPORTED_CERTIFICATE),
            (int)QUIC_STATUS_REVOKED_CERTIFICATE => nameof(QUIC_STATUS_REVOKED_CERTIFICATE),
            (int)QUIC_STATUS_EXPIRED_CERTIFICATE => nameof(QUIC_STATUS_EXPIRED_CERTIFICATE),
            (int)QUIC_STATUS_UNKNOWN_CERTIFICATE => nameof(QUIC_STATUS_UNKNOWN_CERTIFICATE),
            (int)QUIC_STATUS_CERT_EXPIRED => nameof(QUIC_STATUS_CERT_EXPIRED),
            (int)QUIC_STATUS_CERT_UNTRUSTED_ROOT => nameof(QUIC_STATUS_CERT_UNTRUSTED_ROOT),
            _ => null
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int ResolveNameToStatus(string? name)
        => name switch
        {
            "QUIC_STATUS_SUCCESS" => 0,
            nameof(QUIC_STATUS_PENDING) => (int)QUIC_STATUS_PENDING,
            nameof(QUIC_STATUS_CONTINUE) => (int)QUIC_STATUS_CONTINUE,
            nameof(QUIC_STATUS_OUT_OF_MEMORY) => (int)QUIC_STATUS_OUT_OF_MEMORY,
            nameof(QUIC_STATUS_INVALID_PARAMETER) => (int)QUIC_STATUS_INVALID_PARAMETER,
            nameof(QUIC_STATUS_INVALID_STATE) => (int)QUIC_STATUS_INVALID_STATE,
            nameof(QUIC_STATUS_NOT_SUPPORTED) => (int)QUIC_STATUS_NOT_SUPPORTED,
            nameof(QUIC_STATUS_NOT_FOUND) => (int)QUIC_STATUS_NOT_FOUND,
            nameof(QUIC_STATUS_BUFFER_TOO_SMALL) => (int)QUIC_STATUS_BUFFER_TOO_SMALL,
            nameof(QUIC_STATUS_HANDSHAKE_FAILURE) => (int)QUIC_STATUS_HANDSHAKE_FAILURE,
            nameof(QUIC_STATUS_ABORTED) => (int)QUIC_STATUS_ABORTED,
            nameof(QUIC_STATUS_ADDRESS_IN_USE) => (int)QUIC_STATUS_ADDRESS_IN_USE,
            nameof(QUIC_STATUS_CONNECTION_TIMEOUT) => (int)QUIC_STATUS_CONNECTION_TIMEOUT,
            nameof(QUIC_STATUS_CONNECTION_IDLE) => (int)QUIC_STATUS_CONNECTION_IDLE,
            nameof(QUIC_STATUS_UNREACHABLE) => (int)QUIC_STATUS_UNREACHABLE,
            nameof(QUIC_STATUS_INTERNAL_ERROR) => (int)QUIC_STATUS_INTERNAL_ERROR,
            nameof(QUIC_STATUS_CONNECTION_REFUSED) => (int)QUIC_STATUS_CONNECTION_REFUSED,
            nameof(QUIC_STATUS_PROTOCOL_ERROR) => (int)QUIC_STATUS_PROTOCOL_ERROR,
            nameof(QUIC_STATUS_VER_NEG_ERROR) => (int)QUIC_STATUS_VER_NEG_ERROR,
            nameof(QUIC_STATUS_TLS_ERROR) => (int)QUIC_STATUS_TLS_ERROR,
            nameof(QUIC_STATUS_USER_CANCELED) => (int)QUIC_STATUS_USER_CANCELED,
            nameof(QUIC_STATUS_ALPN_NEG_FAILURE) => (int)QUIC_STATUS_ALPN_NEG_FAILURE,
            nameof(QUIC_STATUS_STREAM_LIMIT_REACHED) => (int)QUIC_STATUS_STREAM_LIMIT_REACHED,
            nameof(QUIC_STATUS_CLOSE_NOTIFY) => (int)QUIC_STATUS_CLOSE_NOTIFY,
            nameof(QUIC_STATUS_BAD_CERTIFICATE) => (int)QUIC_STATUS_BAD_CERTIFICATE,
            nameof(QUIC_STATUS_UNSUPPORTED_CERTIFICATE) => (int)QUIC_STATUS_UNSUPPORTED_CERTIFICATE,
            nameof(QUIC_STATUS_REVOKED_CERTIFICATE) => (int)QUIC_STATUS_REVOKED_CERTIFICATE,
            nameof(QUIC_STATUS_EXPIRED_CERTIFICATE) => (int)QUIC_STATUS_EXPIRED_CERTIFICATE,
            nameof(QUIC_STATUS_UNKNOWN_CERTIFICATE) => (int)QUIC_STATUS_UNKNOWN_CERTIFICATE,
            nameof(QUIC_STATUS_CERT_EXPIRED) => (int)QUIC_STATUS_CERT_EXPIRED,
            nameof(QUIC_STATUS_CERT_UNTRUSTED_ROOT) => (int)QUIC_STATUS_CERT_UNTRUSTED_ROOT,
            _ => throw new NotSupportedException(name)
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsSuccess(int status)
        => status == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsPending(int status)
        => status == (int)QUIC_STATUS_PENDING;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsContinue(int status)
        => status == (int)QUIC_STATUS_CONTINUE;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsFailure(int status)
        => status < 0;
}
