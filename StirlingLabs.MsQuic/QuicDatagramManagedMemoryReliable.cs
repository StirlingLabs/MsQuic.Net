using System;
using System.Buffers;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicDatagramManagedMemoryReliable : QuicDatagramReliable
{
    public Memory<byte> Memory { get; }

    public MemoryHandle MemoryHandle { get; }

    private unsafe QUIC_BUFFER* _quicBuffer;
    public override unsafe QUIC_BUFFER* GetBuffer()
    {
        if (_quicBuffer == null)
            _quicBuffer = NativeMemory.New<QUIC_BUFFER>(2);
        _quicBuffer[0].Buffer = null;
        _quicBuffer[0].Length = 0;
        _quicBuffer[1].Buffer = (byte*)MemoryHandle.Pointer;
        _quicBuffer[1].Length = (uint)Memory.Length;
        return _quicBuffer;
    }

    public override unsafe void Dispose()
    {
        base.Dispose();

        if (WipeWhenFinished)
            Memory.Span.Clear();

        MemoryHandle.Dispose();

        if (_quicBuffer == null) return;

        QuicPeerConnection.CleanUpReliableDatagramBuffer(this);

        NativeMemory.Free(_quicBuffer);
    }

    public unsafe QuicDatagramManagedMemoryReliable(QuicPeerConnection connection, Memory<byte> mem, QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        Memory = mem;
        MemoryHandle = Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
}

[PublicAPI]
public sealed class QuicReadOnlyDatagramManagedMemoryReliable : QuicReadOnlyDatagramReliable
{
    public ReadOnlyMemory<byte> Memory { get; }

    public MemoryHandle MemoryHandle { get; }

    private unsafe QUIC_BUFFER* _quicBuffer;
    public override unsafe QUIC_BUFFER* GetBuffer()
    {
        if (_quicBuffer == null)
            _quicBuffer = NativeMemory.New<QUIC_BUFFER>(2);
        _quicBuffer[0].Buffer = null;
        _quicBuffer[0].Length = 0;
        _quicBuffer[1].Buffer = (byte*)MemoryHandle.Pointer;
        _quicBuffer[1].Length = (uint)Memory.Length;
        return _quicBuffer;
    }

    public override unsafe void Dispose()
    {
        base.Dispose();

        MemoryHandle.Dispose();

        if (_quicBuffer == null) return;

        QuicPeerConnection.CleanUpReliableDatagramBuffer(this);

        NativeMemory.Free(_quicBuffer);
    }

    public unsafe QuicReadOnlyDatagramManagedMemoryReliable(QuicPeerConnection connection, ReadOnlyMemory<byte> mem,
        QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        Memory = mem;
        MemoryHandle = Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
}
