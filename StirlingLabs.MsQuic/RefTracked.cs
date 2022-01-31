using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Quic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public struct RefTrackedValue<T>
{
    public T Value;

#if NET5_0_OR_GREATER
    public int RefCount
    {
        get => _refCount;
        init => _refCount = value;
    }
#else
    public int RefCount
    {
        get => _refCount;
        set => _refCount = value;
    }
#endif

    private int _refCount = 0;
    public unsafe void Release()
    {
        if (Interlocked.Decrement(ref _refCount) == 0)
            Marshal.FreeHGlobal((IntPtr)Unsafe.AsPointer(ref this));
    }

    public override bool Equals(object? obj)
        => false;

    public override unsafe int GetHashCode()
        => ((IntPtr)Unsafe.AsPointer(ref Unsafe.AsRef(this))).GetHashCode();

    public static bool operator ==(in RefTrackedValue<T> left, in RefTrackedValue<T> right)
        => Unsafe.AreSame(ref Unsafe.AsRef(left), ref Unsafe.AsRef(right));

    public static bool operator !=(in RefTrackedValue<T> left, in RefTrackedValue<T> right)
        => !Unsafe.AreSame(ref Unsafe.AsRef(left), ref Unsafe.AsRef(right));
}
