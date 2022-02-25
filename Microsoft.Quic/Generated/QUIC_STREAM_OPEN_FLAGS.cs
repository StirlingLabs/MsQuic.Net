//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace Microsoft.Quic
{
    [Flags]
    public enum QUIC_STREAM_OPEN_FLAGS
    {
        QUIC_STREAM_OPEN_FLAG_NONE = 0x0000,
        QUIC_STREAM_OPEN_FLAG_UNIDIRECTIONAL = 0x0001,
        QUIC_STREAM_OPEN_FLAG_0_RTT = 0x0002,
    }
}
