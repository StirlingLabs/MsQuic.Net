//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public partial struct QUIC_HANDSHAKE_INFO
    {
        public QUIC_TLS_PROTOCOL_VERSION TlsProtocolVersion;

        public QUIC_CIPHER_ALGORITHM CipherAlgorithm;

        [NativeTypeName("int32_t")]
        public int CipherStrength;

        public QUIC_HASH_ALGORITHM Hash;

        [NativeTypeName("int32_t")]
        public int HashStrength;

        public QUIC_KEY_EXCHANGE_ALGORITHM KeyExchangeAlgorithm;

        [NativeTypeName("int32_t")]
        public int KeyExchangeStrength;

        public QUIC_CIPHER_SUITE CipherSuite;
    }
}
