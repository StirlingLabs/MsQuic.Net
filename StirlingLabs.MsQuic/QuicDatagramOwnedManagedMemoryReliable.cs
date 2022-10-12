using System;
using System.Buffers;
using JetBrains.Annotations;
using StirlingLabs.MsQuic.Bindings;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicDatagramOwnedManagedMemoryReliable : QuicDatagram
{
    public IMemoryOwner<byte> MemoryOwner { get; }
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

    public override void Dispose()
    {
        base.Dispose();

        if (WipeWhenFinished)
            Memory.Span.Clear();

        MemoryHandle.Dispose();
        MemoryOwner.Dispose();
    }

    public unsafe QuicDatagramOwnedManagedMemoryReliable(QuicPeerConnection connection, IMemoryOwner<byte> mem,
        QUIC_DATAGRAM_SEND_STATE state = QUIC_DATAGRAM_SEND_STATE.UNKNOWN)
        : base(connection, state)
    {
        MemoryOwner = mem;
        Memory = MemoryOwner.Memory;
        MemoryHandle = MemoryOwner.Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
    public unsafe QuicDatagramOwnedManagedMemoryReliable(QuicPeerConnection connection, IMemoryOwner<byte> memOwner, Memory<byte> mem,
        QUIC_DATAGRAM_SEND_STATE state = QUIC_DATAGRAM_SEND_STATE.UNKNOWN)
        : base(connection, state)
    {
        MemoryOwner = memOwner;
        Memory = mem;
        MemoryHandle = MemoryOwner.Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
}
