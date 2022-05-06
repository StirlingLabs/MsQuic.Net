//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace StirlingLabs.MsQuic.Bindings
{
    [Flags]
    public enum QUIC_CONNECTION_SHUTDOWN_FLAGS
    {
        QUIC_CONNECTION_SHUTDOWN_FLAG_NONE = 0x0000,
        QUIC_CONNECTION_SHUTDOWN_FLAG_SILENT = 0x0001,
    }
}
