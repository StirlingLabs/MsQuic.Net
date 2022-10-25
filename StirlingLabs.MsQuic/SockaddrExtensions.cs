using System.Net;
using System.Runtime.CompilerServices;
using StirlingLabs.MsQuic.Bindings;

namespace StirlingLabs.MsQuic;

public static class SockaddrExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref sockaddr AsSockaddr(this ref QuicAddr p)
        => ref Unsafe.As<QuicAddr, sockaddr>(ref Unsafe.AsRef(p));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref sockaddr AsSockaddr(this ref Bindings.sockaddr p)
        => ref Unsafe.As<Bindings.sockaddr, sockaddr>(ref Unsafe.AsRef(p));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly sockaddr AsReadOnlySockaddr(this in QuicAddr p)
        => ref Unsafe.As<QuicAddr, sockaddr>(ref Unsafe.AsRef(p));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly sockaddr AsReadOnlySockaddr(this in Bindings.sockaddr p)
        => ref Unsafe.As<Bindings.sockaddr, sockaddr>(ref Unsafe.AsRef(p));
    

    // TODO: move to StirlingLabs.sockaddr.Net
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe IPEndPoint ToEndPoint(in this sockaddr r)
    {
        var sockaddrPtr = (sockaddr*) Unsafe.AsPointer(ref Unsafe.AsRef(r));
        return new((*sockaddrPtr).GetIPAddress(), (*sockaddrPtr).GetPort());
    }
}
