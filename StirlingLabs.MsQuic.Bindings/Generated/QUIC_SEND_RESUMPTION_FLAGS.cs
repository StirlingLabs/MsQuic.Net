//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace StirlingLabs.MsQuic.Bindings
{
    [Flags]
    public enum QUIC_SEND_RESUMPTION_FLAGS
    {
        QUIC_SEND_RESUMPTION_FLAG_NONE = 0x0000,
        QUIC_SEND_RESUMPTION_FLAG_FINAL = 0x0001,
    }
}
