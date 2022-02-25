//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public enum QUIC_STREAM_SCHEDULING_SCHEME
    {
        QUIC_STREAM_SCHEDULING_SCHEME_FIFO = 0x0000,
        QUIC_STREAM_SCHEDULING_SCHEME_ROUND_ROBIN = 0x0001,
        QUIC_STREAM_SCHEDULING_SCHEME_COUNT,
    }
}
