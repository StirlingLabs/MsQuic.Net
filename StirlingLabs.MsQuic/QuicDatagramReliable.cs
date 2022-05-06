using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using StirlingLabs.MsQuic.Bindings;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public abstract class QuicDatagramReliable : QuicDatagram, IQuicDatagramReliable
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
    public event Utilities.EventHandler<IQuicDatagramReliable>? ReliablyAcknowledged;

    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off")]
    [SuppressMessage("Design", "CA1033", Justification = "Invoked through interface")]
    void IQuicDatagramReliable.OnReliablyAcknowledged()
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
    public event Utilities.EventHandler<IQuicDatagramReliable>? Retransmitting;

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
    [SuppressMessage("Design", "CA1063", Justification = "It's fine")]
    public override void Dispose()
    {
        Connection.RemoveFromUnacknowledged(this);

        base.Dispose();
    }
}
