//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace StirlingLabs.MsQuic.Bindings
{
    public enum QUIC_DATAGRAM_SEND_STATE
    {
        QUIC_DATAGRAM_SEND_UNKNOWN,
        QUIC_DATAGRAM_SEND_SENT,
        QUIC_DATAGRAM_SEND_LOST_SUSPECT,
        QUIC_DATAGRAM_SEND_LOST_DISCARDED,
        QUIC_DATAGRAM_SEND_ACKNOWLEDGED,
        QUIC_DATAGRAM_SEND_ACKNOWLEDGED_SPURIOUS,
        QUIC_DATAGRAM_SEND_CANCELED,
    }
}
