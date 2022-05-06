#if NETSTANDARD2_0 || NETSTANDARD2_1
using System;
using System.Runtime.CompilerServices;
using SystemOSPlatform = System.Runtime.InteropServices.OSPlatform;

namespace StirlingLabs.MsQuic.Bindings;

public static class OSPlatform
{
    public static SystemOSPlatform FreeBSD { get; } = SystemOSPlatform.Create("FREEBSD");

    public static SystemOSPlatform Linux => SystemOSPlatform.Linux;

    public static SystemOSPlatform OSX => SystemOSPlatform.OSX;

    public static SystemOSPlatform Windows => SystemOSPlatform.Windows;
}

#endif
