//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace StirlingLabs.MsQuic.Bindings
{
    [Flags]
    public enum QUIC_STREAM_START_FLAGS
    {
        QUIC_STREAM_START_FLAG_NONE = 0x0000,
        QUIC_STREAM_START_FLAG_IMMEDIATE = 0x0001,
        QUIC_STREAM_START_FLAG_FAIL_BLOCKED = 0x0002,
        QUIC_STREAM_START_FLAG_SHUTDOWN_ON_FAIL = 0x0004,
        QUIC_STREAM_START_FLAG_INDICATE_PEER_ACCEPT = 0x0008,
    }
}
