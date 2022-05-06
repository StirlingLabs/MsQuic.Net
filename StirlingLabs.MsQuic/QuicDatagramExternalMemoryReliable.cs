using System;
using JetBrains.Annotations;
using StirlingLabs.MsQuic.Bindings;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicDatagramExternalMemoryReliable : QuicDatagramReliable
{
    private unsafe byte* _externalMemStart;
    private uint _externalMemLength;
    private readonly Action<IntPtr> _externalMemFree;

    private unsafe QUIC_BUFFER* _quicBuffer;

    public override unsafe QUIC_BUFFER* GetBuffer()
    {
        if (_quicBuffer == null)
            _quicBuffer = NativeMemory.New<QUIC_BUFFER>(2);
        _quicBuffer[0].Buffer = null;
        _quicBuffer[0].Length = 0;
        _quicBuffer[1].Buffer = _externalMemStart;
        _quicBuffer[1].Length = _externalMemLength;
        return _quicBuffer;
    }

    public override unsafe void Dispose()
    {
        base.Dispose();

        _externalMemFree((IntPtr)_externalMemStart);
        _externalMemStart = null;
        _externalMemLength = 0;

        if (_quicBuffer == null) return;

        QuicPeerConnection.CleanUpReliableDatagramBuffer(this);

        NativeMemory.Free(_quicBuffer);

    }

    public unsafe QuicDatagramExternalMemoryReliable(QuicPeerConnection connection, byte* pStart, uint length, Action<IntPtr> freeCallback,
        QUIC_DATAGRAM_SEND_STATE state = Unknown)
        : base(connection, state)
    {
        _externalMemStart = pStart;
        _externalMemLength = length;
        _externalMemFree = freeCallback;
    }
}
