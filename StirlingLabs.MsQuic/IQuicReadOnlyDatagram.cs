using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Quic;
using StirlingLabs.Utilities;

namespace StirlingLabs.MsQuic;

public interface IQuicReadOnlyDatagram : IDisposable
{
    QuicPeerConnection Connection { get; }

    GCHandle GcHandle { get; }

    QUIC_DATAGRAM_SEND_STATE State { get; set; }

    QUIC_SEND_FLAGS Flags { get; set; }

    bool IsSent { get; }

    bool IsCanceled { get; }

    bool IsAcknowledged { get; }

    bool IsLost { get; }

    bool IsDiscarded { get; }

    unsafe QUIC_BUFFER* GetBuffer();

    Task WaitForSentAsync();

    Task WaitForAcknowledgementAsync();

    bool Send();

    bool RetrySend();

    event EventHandler<IQuicReadOnlyDatagram, QUIC_DATAGRAM_SEND_STATE>? StateChanged;


    event EventHandler<IQuicReadOnlyDatagram, ExceptionDispatchInfo>? UnobservedException;
}
