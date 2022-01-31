using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Quic;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public unsafe partial struct QUIC_BUFFER
{
    public Span<byte> Span => new(Buffer, (int)Length);
    public ReadOnlySpan<byte> ReadOnlySpan => new(Buffer, (int)Length);

    [SuppressMessage("Usage", "CA2225", Justification = "See Span")]
    public static implicit operator Span<byte>(QUIC_BUFFER buffer) => buffer.Span;

    [SuppressMessage("Usage", "CA2225", Justification = "See ReadOnlySpan")]
    public static implicit operator ReadOnlySpan<byte>(QUIC_BUFFER buffer) => buffer.Span;
}
