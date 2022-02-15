using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using StirlingLabs.Utilities;

namespace StirlingLabs.MsQuic;

[PublicAPI]
[ExcludeFromCodeCoverage]
internal static class LogTimeStamp
{
    public static void Init()
    {
        // run static init
    }

    public static Timestamp InitTime = Timestamp.Now;

    public static double ElapsedSeconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Timestamp.Now - InitTime;
    }

    public static TimeSpan Elapsed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TimeSpan.FromSeconds(ElapsedSeconds);
    }
}
