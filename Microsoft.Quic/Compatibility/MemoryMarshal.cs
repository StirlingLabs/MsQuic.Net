#if NETSTANDARD2_0
using System;
using System.Runtime.CompilerServices;
using SystemOSPlatform = System.Runtime.InteropServices.OSPlatform;

namespace Microsoft.Quic;

public static class MemoryMarshal
{
    /// <summary>
    /// Create a new span over a portion of a regular managed object. This can be useful
    /// if part of a managed object represents a "fixed array." This is dangerous because the
    /// <paramref name="length"/> is not checked.
    /// </summary>
    /// <param name="reference">A reference to data.</param>
    /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
    /// <returns>The lifetime of the returned span will not be validated for safety by span-aware languages.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpan<T>(ref T reference, int length) => new(Unsafe.AsPointer(ref reference), length);

    /// <summary>
    /// Create a new read-only span over a portion of a regular managed object. This can be useful
    /// if part of a managed object represents a "fixed array." This is dangerous because the
    /// <paramref name="length"/> is not checked.
    /// </summary>
    /// <param name="reference">A reference to data.</param>
    /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
    /// <returns>The lifetime of the returned span will not be validated for safety by span-aware languages.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length) => new(Unsafe.AsPointer(ref reference), length);


    /// <summary>
    /// Returns a reference to the 0th element of the Span. If the Span is empty, returns a reference to the location where the 0th element
    /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
    /// </summary>
    public static ref T GetReference<T>(Span<T> span) => ref span.GetPinnableReference();

    /// <summary>
    /// Returns a reference to the 0th element of the ReadOnlySpan. If the ReadOnlySpan is empty, returns a reference to the location where the 0th element
    /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
    /// </summary>
    public static ref T GetReference<T>(ReadOnlySpan<T> span) => ref Unsafe.AsRef(span.GetPinnableReference());
}

#endif
