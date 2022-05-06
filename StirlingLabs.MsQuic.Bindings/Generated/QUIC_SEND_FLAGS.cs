//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace StirlingLabs.MsQuic.Bindings
{
    [Flags]
    public enum QUIC_SEND_FLAGS
    {
        QUIC_SEND_FLAG_NONE = 0x0000,
        QUIC_SEND_FLAG_ALLOW_0_RTT = 0x0001,
        QUIC_SEND_FLAG_START = 0x0002,
        QUIC_SEND_FLAG_FIN = 0x0004,
        QUIC_SEND_FLAG_DGRAM_PRIORITY = 0x0008,
        QUIC_SEND_FLAG_DELAY_SEND = 0x0010,
    }
}
