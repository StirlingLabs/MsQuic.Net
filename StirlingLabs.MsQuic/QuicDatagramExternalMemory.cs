using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicDatagramExternalMemory : QuicDatagram
{
    private unsafe byte* _externalMemStart;
    private uint _externalMemLength;
    private readonly Action<IntPtr> _externalMemFree;

    public MemoryHandle MemoryHandle { get; }

    private unsafe QUIC_BUFFER* _quicBuffer;

    public override unsafe QUIC_BUFFER* GetBuffer()
    {
        if (_quicBuffer == null)
            _quicBuffer = NativeMemory.New<QUIC_BUFFER>();
        _quicBuffer->Buffer = _externalMemStart;
        _quicBuffer->Length = _externalMemLength;
        return _quicBuffer;
    }

    public override unsafe void Dispose()
    {
        base.Dispose();

        _externalMemFree((IntPtr)_externalMemStart);
        _externalMemStart = null;
        _externalMemLength = 0;
    }

    public unsafe QuicDatagramExternalMemory(QuicPeerConnection connection, byte* pStart, uint length, Action<IntPtr> freeCallback,
        QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        _externalMemStart = pStart;
        _externalMemLength = length;
        _externalMemFree = freeCallback;
        NativeMemory.Free(_quicBuffer);
    }
}
