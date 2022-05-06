using System;
using System.Buffers;
using JetBrains.Annotations;
using StirlingLabs.MsQuic.Bindings;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicDatagramManagedMemory : QuicDatagram
{
    public Memory<byte> Memory { get; }

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
    }

    public unsafe QuicDatagramManagedMemory(QuicPeerConnection connection, Memory<byte> mem, QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        Memory = mem;
        MemoryHandle = Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
}

[PublicAPI]
public sealed class QuicReadOnlyDatagramManagedMemory : QuicReadOnlyDatagram
{
    public ReadOnlyMemory<byte> Memory { get; }

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

        MemoryHandle.Dispose();
    }

    public unsafe QuicReadOnlyDatagramManagedMemory(QuicPeerConnection connection, ReadOnlyMemory<byte> mem,
        QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        Memory = mem;
        MemoryHandle = Memory.Pin();
        NativeMemory.Free(_quicBuffer);
    }
}
