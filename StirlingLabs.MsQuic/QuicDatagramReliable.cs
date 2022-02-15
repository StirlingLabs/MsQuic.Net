using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using Microsoft.Quic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public abstract class QuicDatagramReliable
    : QuicDatagram
{
    protected QuicDatagramReliable(QuicPeerConnection connection, QUIC_DATAGRAM_SEND_STATE state)
        : base(connection, state)
        => Id = connection.GenerateReliableId();

    protected internal object Lock = new();

    public ulong Id { get; protected set; }

    public bool IsReliablyAcknowledged { get; internal set; }

    public ulong MaximumRetransmits { get; set; }

    public ulong Retransmits { get; internal set; }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event Utilities.EventHandler<QuicDatagram>? ReliablyAcknowledged;

    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off")]
    protected internal virtual void OnReliablyAcknowledged()
    {
        IsReliablyAcknowledged = true;
        try
        {
            ReliablyAcknowledged?.Invoke(this);
        }
        catch (Exception ex)
        {
            OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
        }
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event Utilities.EventHandler<QuicDatagram>? Retransmitting;

    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off")]
    protected internal virtual void OnRetransmitting()
    {
        ++Retransmits;
        try
        {
            Retransmitting?.Invoke(this);
        }
        catch (Exception ex)
        {
            OnUnobservedException(ExceptionDispatchInfo.Capture(ex));
        }
    }

    [SuppressMessage("Usage", "CA1816", Justification = "Done")]
    public override void Dispose()
    {
        Connection.RemoveFromUnacknowledged(this);

        base.Dispose();
    }
}
