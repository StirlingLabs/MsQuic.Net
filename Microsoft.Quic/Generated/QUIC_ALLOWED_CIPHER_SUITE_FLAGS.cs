//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace Microsoft.Quic
{
    [Flags]
    public enum QUIC_ALLOWED_CIPHER_SUITE_FLAGS
    {
        QUIC_ALLOWED_CIPHER_SUITE_NONE = 0x0,
        QUIC_ALLOWED_CIPHER_SUITE_AES_128_GCM_SHA256 = 0x1,
        QUIC_ALLOWED_CIPHER_SUITE_AES_256_GCM_SHA384 = 0x2,
        QUIC_ALLOWED_CIPHER_SUITE_CHACHA20_POLY1305_SHA256 = 0x4,
    }
}
