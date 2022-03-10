using System.Runtime.CompilerServices;
using Microsoft.Quic;
using StirlingLabs.Utilities;

namespace StirlingLabs.MsQuic;

public static class QuicBufferExtensions {

    public static unsafe BigSpan<byte> GetBigSpan(ref this QUIC_BUFFER r)
    {
        var p = (QUIC_BUFFER*) Unsafe.AsPointer(ref r);
        return new(p->Buffer, p->Length);
    }

    public static unsafe ReadOnlyBigSpan<byte> GetReadOnlyBigSpan(ref this QUIC_BUFFER r)
    {
        var p = (QUIC_BUFFER*) Unsafe.AsPointer(ref r);
        return new(p->Buffer, p->Length);
    }
}
