//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace StirlingLabs.MsQuic.Bindings
{
    [Flags]
    public enum QUIC_STREAM_SHUTDOWN_FLAGS
    {
        QUIC_STREAM_SHUTDOWN_FLAG_NONE = 0x0000,
        QUIC_STREAM_SHUTDOWN_FLAG_GRACEFUL = 0x0001,
        QUIC_STREAM_SHUTDOWN_FLAG_ABORT_SEND = 0x0002,
        QUIC_STREAM_SHUTDOWN_FLAG_ABORT_RECEIVE = 0x0004,
        QUIC_STREAM_SHUTDOWN_FLAG_ABORT = 0x0006,
        QUIC_STREAM_SHUTDOWN_FLAG_IMMEDIATE = 0x0008,
        QUIC_STREAM_SHUTDOWN_FLAG_INLINE = 0x0010,
    }
}
