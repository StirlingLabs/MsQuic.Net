using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Quic;
using StirlingLabs.Utilities;

namespace StirlingLabs.MsQuic;

public interface IQuicDatagramReliable : IQuicReadOnlyDatagram, IDisposable
{
    ulong Id { get; }
    bool IsReliablyAcknowledged { get; }
    ulong MaximumRetransmits { get; set; }
    ulong Retransmits { get; }

    event Utilities.EventHandler<IQuicDatagramReliable>? ReliablyAcknowledged;

    event Utilities.EventHandler<IQuicDatagramReliable>? Retransmitting;

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    protected internal
#endif
        void OnReliablyAcknowledged();
}
