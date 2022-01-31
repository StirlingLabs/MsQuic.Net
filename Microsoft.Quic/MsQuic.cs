//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;

namespace Microsoft.Quic;

[PublicAPI]
[SuppressMessage("Security", "CA5392", Justification = "Manual initialization")]
[SuppressMessage("Design", "CA1060", Justification = "Generated code")]
public partial class MsQuic
{
    public const string LibName = "msquic-openssl";

#if NET5_0_OR_GREATER
    [SuppressMessage("Design", "CA1065", Justification = "Security critical failure")]
    static MsQuic()
    {
        var asmDir = new FileInfo(new Uri(typeof(MsQuic).Assembly.Location).LocalPath)
            .Directory!.FullName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var msquicPath = Path.Combine(asmDir, "msquic-openssl.dll");
            if (!NativeLibrary.TryLoad(msquicPath, typeof(MsQuic).Assembly, DllImportSearchPath.LegacyBehavior, out _))
                throw new DllNotFoundException("Missing MsQuic native library.");
        }
        else
        {
            var extension = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? ".dylib"
                : ".so";
            var msquicPath = Path.Combine(asmDir, "libmsquic-openssl" + extension);
            if (!NativeLibrary.TryLoad(msquicPath, typeof(MsQuic).Assembly, DllImportSearchPath.AssemblyDirectory, out _))
                throw new DllNotFoundException("Missing MsQuic native library.");
        }
    }
#endif

    public static void Init() { }

    public static unsafe QUIC_API_TABLE* Open()
    {
        QUIC_API_TABLE* apiTable;
        AssertSuccess(MsQuicOpenVersion(1, (void**)&apiTable));
        return apiTable;
    }

    public static unsafe void Close(QUIC_API_TABLE* apiTable)
        => MsQuicClose(apiTable);

    public static void AssertSuccess(int status)
        => Assert(IsSuccess(status), status);

    public static void AssertNotFailure(int status)
        => Assert(!IsFailure(status), status);

    [AssertionMethod]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, int status)
    {
        if (!condition) throw new QuicException(status);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSuccess(int status)
        => MsQuicStatusBaseImpl.IsSuccess(status);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPending(int status)
        => MsQuicStatusBaseImpl.IsPending(status);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsContinue(int status)
        => MsQuicStatusBaseImpl.IsContinue(status);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFailure(int status)
        => MsQuicStatusBaseImpl.IsFailure(status);

    private static readonly MsQuicStatusBase MsQuicStatusBaseImpl
        = MsQuicStatus.GetPlatformSpecificImplementation();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetNameForStatus(int status)
        => MsQuicStatusBaseImpl.GetNameForStatus(status);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ResolveNameToStatus(string? name)
        => MsQuicStatusBaseImpl.ResolveNameToStatus(name);

    // ReSharper disable InconsistentNaming
    public static readonly int QUIC_STATUS_SUCCESS = 0;
    public static readonly int QUIC_STATUS_PENDING = ResolveNameToStatus(nameof(QUIC_STATUS_PENDING));
    public static readonly int QUIC_STATUS_CONTINUE = ResolveNameToStatus(nameof(QUIC_STATUS_CONTINUE));
    public static readonly int QUIC_STATUS_OUT_OF_MEMORY = ResolveNameToStatus(nameof(QUIC_STATUS_OUT_OF_MEMORY));
    public static readonly int QUIC_STATUS_INVALID_PARAMETER = ResolveNameToStatus(nameof(QUIC_STATUS_INVALID_PARAMETER));
    public static readonly int QUIC_STATUS_INVALID_STATE = ResolveNameToStatus(nameof(QUIC_STATUS_INVALID_STATE));
    public static readonly int QUIC_STATUS_NOT_SUPPORTED = ResolveNameToStatus(nameof(QUIC_STATUS_NOT_SUPPORTED));
    public static readonly int QUIC_STATUS_NOT_FOUND = ResolveNameToStatus(nameof(QUIC_STATUS_NOT_FOUND));
    public static readonly int QUIC_STATUS_BUFFER_TOO_SMALL = ResolveNameToStatus(nameof(QUIC_STATUS_BUFFER_TOO_SMALL));
    public static readonly int QUIC_STATUS_HANDSHAKE_FAILURE = ResolveNameToStatus(nameof(QUIC_STATUS_HANDSHAKE_FAILURE));
    public static readonly int QUIC_STATUS_ABORTED = ResolveNameToStatus(nameof(QUIC_STATUS_ABORTED));
    public static readonly int QUIC_STATUS_ADDRESS_IN_USE = ResolveNameToStatus(nameof(QUIC_STATUS_ADDRESS_IN_USE));
    public static readonly int QUIC_STATUS_CONNECTION_TIMEOUT = ResolveNameToStatus(nameof(QUIC_STATUS_CONNECTION_TIMEOUT));
    public static readonly int QUIC_STATUS_CONNECTION_IDLE = ResolveNameToStatus(nameof(QUIC_STATUS_CONNECTION_IDLE));
    public static readonly int QUIC_STATUS_UNREACHABLE = ResolveNameToStatus(nameof(QUIC_STATUS_UNREACHABLE));
    public static readonly int QUIC_STATUS_INTERNAL_ERROR = ResolveNameToStatus(nameof(QUIC_STATUS_INTERNAL_ERROR));
    public static readonly int QUIC_STATUS_CONNECTION_REFUSED = ResolveNameToStatus(nameof(QUIC_STATUS_CONNECTION_REFUSED));
    public static readonly int QUIC_STATUS_PROTOCOL_ERROR = ResolveNameToStatus(nameof(QUIC_STATUS_PROTOCOL_ERROR));
    public static readonly int QUIC_STATUS_VER_NEG_ERROR = ResolveNameToStatus(nameof(QUIC_STATUS_VER_NEG_ERROR));
    public static readonly int QUIC_STATUS_TLS_ERROR = ResolveNameToStatus(nameof(QUIC_STATUS_TLS_ERROR));
    public static readonly int QUIC_STATUS_USER_CANCELED = ResolveNameToStatus(nameof(QUIC_STATUS_USER_CANCELED));
    public static readonly int QUIC_STATUS_ALPN_NEG_FAILURE = ResolveNameToStatus(nameof(QUIC_STATUS_ALPN_NEG_FAILURE));
    public static readonly int QUIC_STATUS_STREAM_LIMIT_REACHED = ResolveNameToStatus(nameof(QUIC_STATUS_STREAM_LIMIT_REACHED));
    public static readonly int QUIC_STATUS_CLOSE_NOTIFY = ResolveNameToStatus(nameof(QUIC_STATUS_CLOSE_NOTIFY));
    public static readonly int QUIC_STATUS_BAD_CERTIFICATE = ResolveNameToStatus(nameof(QUIC_STATUS_BAD_CERTIFICATE));
    public static readonly int QUIC_STATUS_UNSUPPORTED_CERTIFICATE = ResolveNameToStatus(nameof(QUIC_STATUS_UNSUPPORTED_CERTIFICATE));
    public static readonly int QUIC_STATUS_REVOKED_CERTIFICATE = ResolveNameToStatus(nameof(QUIC_STATUS_REVOKED_CERTIFICATE));
    public static readonly int QUIC_STATUS_EXPIRED_CERTIFICATE = ResolveNameToStatus(nameof(QUIC_STATUS_EXPIRED_CERTIFICATE));
    public static readonly int QUIC_STATUS_UNKNOWN_CERTIFICATE = ResolveNameToStatus(nameof(QUIC_STATUS_UNKNOWN_CERTIFICATE));
    public static readonly int QUIC_STATUS_CERT_EXPIRED = ResolveNameToStatus(nameof(QUIC_STATUS_CERT_EXPIRED));
    public static readonly int QUIC_STATUS_CERT_UNTRUSTED_ROOT = ResolveNameToStatus(nameof(QUIC_STATUS_CERT_UNTRUSTED_ROOT));
    // ReSharper restore InconsistentNaming
}
