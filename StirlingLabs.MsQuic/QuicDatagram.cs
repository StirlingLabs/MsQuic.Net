using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Quic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public abstract class QuicDatagram : QuicReadOnlyDatagram, IQuicDatagram
{
    public static QuicDatagram Create(QuicPeerConnection connection, Memory<byte> data)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        if (!CanCreate(connection, data))
            throw new ArgumentOutOfRangeException(nameof(data), "Message size too large to fit into a datagram.");

        return connection.DatagramsAreReliable
            ? new QuicDatagramManagedMemoryReliable(connection, data)
            : new QuicDatagramManagedMemory(connection, data);
    }
    public static QuicDatagram Create(QuicPeerConnection connection, IMemoryOwner<byte> owner, Memory<byte> data)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        if (!CanCreate(connection, data))
            throw new ArgumentOutOfRangeException(nameof(data), "Message size too large to fit into a datagram.");

        return connection.DatagramsAreReliable
            ? new QuicDatagramOwnedManagedMemoryReliable(connection, owner, data)
            : new QuicDatagramOwnedManagedMemory(connection, owner, data);
    }
    public static unsafe QuicDatagram Create(QuicPeerConnection connection, byte* pExternalMemStart, uint externalMemLength,
        Action<IntPtr> freeCallback)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        if (!CanCreate(connection, externalMemLength))
            throw new ArgumentOutOfRangeException(nameof(externalMemLength), "Message size too large to fit into a datagram.");

        return connection.DatagramsAreReliable
            ? new QuicDatagramExternalMemoryReliable(connection, pExternalMemStart, externalMemLength, freeCallback)
            : new QuicDatagramExternalMemory(connection, pExternalMemStart, externalMemLength, freeCallback);
    }

    protected QuicDatagram(QuicPeerConnection connection, QUIC_DATAGRAM_SEND_STATE state) : base(connection, state) { }

    public bool WipeWhenFinished { get; set; }
}
