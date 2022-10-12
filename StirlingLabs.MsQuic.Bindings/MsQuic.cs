#nullable enable
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
using StirlingLabs.Utilities;

#if NET6_0_OR_GREATER
using NativeLibrary = System.Runtime.InteropServices.NativeLibrary;
#endif

namespace StirlingLabs.MsQuic.Bindings;

[PublicAPI]
[SuppressMessage("Security", "CA5392", Justification = "Manual initialization")]
[SuppressMessage("Design", "CA1060", Justification = "They're in generated code")]
public partial class MsQuic
{
    public const string LibName = "msquic-openssl";

    [SuppressMessage("Design", "CA1065", Justification = "Security critical failure")]
    static MsQuic()
    {
        var asmDir = new FileInfo(new Uri(typeof(MsQuic).Assembly.Location).LocalPath)
            .Directory!.FullName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var msquicPath = Path.Combine(asmDir, "msquic-openssl.dll");
            NativeLibrary.Load(msquicPath, typeof(MsQuic).Assembly, DllImportSearchPath.LegacyBehavior);
        }
        else
        {
            var extension = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? ".dylib"
                : ".so";
            var msquicPath = Path.Combine(asmDir, "libmsquic-openssl" + extension);
            NativeLibrary.Load(msquicPath, typeof(MsQuic).Assembly, DllImportSearchPath.AssemblyDirectory);
        }
    }

    public static void Init() { }

    public static void AssertSuccess(int status)
        => Assert(StatusSucceeded(status), status);

    public static void AssertNotFailure(int status)
        => Assert(!StatusFailed(status), status);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPending(int status)
        => status == QUIC_STATUS_PENDING;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsContinue(int status)
        => status == QUIC_STATUS_CONTINUE;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSuccess(int status)
        => StatusSucceeded(status);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFailure(int status)
        => StatusFailed(status);

    [AssertionMethod]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, int status)
    {
        if (!condition) throw new MsQuicException(status);
    }
}
