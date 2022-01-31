using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.MsQuic;

[PublicAPI]
internal static class TimeStamp
{
    public static long StartUpTicks = Stopwatch.GetTimestamp();

    public static long ElapsedTicks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stopwatch.GetTimestamp() - StartUpTicks;
    }

    public static TimeSpan Elapsed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(ElapsedTicks);
    }
}
