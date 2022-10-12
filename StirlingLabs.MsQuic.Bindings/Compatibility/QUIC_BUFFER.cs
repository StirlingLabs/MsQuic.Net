using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace StirlingLabs.MsQuic.Bindings;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public unsafe partial struct QUIC_BUFFER : IEquatable<QUIC_BUFFER>
{
    public ReadOnlySpan<byte> ReadOnlySpan => new(Buffer, (int)Length);

    [SuppressMessage("Usage", "CA2225", Justification = "See Span")]
    public static implicit operator Span<byte>(QUIC_BUFFER buffer) => buffer.Span;

    [SuppressMessage("Usage", "CA2225", Justification = "See ReadOnlySpan")]
    public static implicit operator ReadOnlySpan<byte>(QUIC_BUFFER buffer) => buffer.Span;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(QUIC_BUFFER other)
        => Buffer == other.Buffer
            && Length == other.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(QUIC_BUFFER left, QUIC_BUFFER right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(QUIC_BUFFER left, QUIC_BUFFER right)
        => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => ((int)Length * 397) ^ ((nint)Buffer).GetHashCode();
}
