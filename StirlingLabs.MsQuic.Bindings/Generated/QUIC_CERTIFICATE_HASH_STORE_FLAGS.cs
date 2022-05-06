//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace StirlingLabs.MsQuic.Bindings
{
    [Flags]
    public enum QUIC_CERTIFICATE_HASH_STORE_FLAGS
    {
        QUIC_CERTIFICATE_HASH_STORE_FLAG_NONE = 0x0000,
        QUIC_CERTIFICATE_HASH_STORE_FLAG_MACHINE_STORE = 0x0001,
    }
}
