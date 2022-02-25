//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public enum QUIC_CIPHER_SUITE
    {
        QUIC_CIPHER_SUITE_TLS_AES_128_GCM_SHA256 = 0x1301,
        QUIC_CIPHER_SUITE_TLS_AES_256_GCM_SHA384 = 0x1302,
        QUIC_CIPHER_SUITE_TLS_CHACHA20_POLY1305_SHA256 = 0x1303,
    }
}
