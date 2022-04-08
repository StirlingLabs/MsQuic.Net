using System;
using System.Buffers;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicDatagramOwnedManagedMemory : QuicDatagram
{
    public IMemoryOwner<byte> MemoryOwner { get; }
    public Memory<byte> Memory { get; set; }

    public MemoryHandle MemoryHandle { get; }

    private unsafe QUIC_BUFFER* _quicBuffer;
    public override unsafe QUIC_BUFFER* GetBuffer()
    {
        if (_quicBuffer == null)
            _quicBuffer = NativeMemory.New<QUIC_BUFFER>();
        _quicBuffer->Buffer = (byte*)MemoryHandle.Pointer;
        _quicBuffer->Length = (uint)Memory.Length;
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

    public unsafe QuicDatagramOwnedManagedMemory(QuicPeerConnection connection, IMemoryOwner<byte> mem,
        QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        MemoryOwner = mem;
        Memory = MemoryOwner.Memory;
        MemoryHandle = MemoryOwner.Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
    public unsafe QuicDatagramOwnedManagedMemory(QuicPeerConnection connection, IMemoryOwner<byte> memOwner, Memory<byte> mem,
        QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        MemoryOwner = memOwner;
        Memory = mem;
        MemoryHandle = MemoryOwner.Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
}
