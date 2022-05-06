//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace StirlingLabs.MsQuic.Bindings
{
    public unsafe partial struct QUIC_NEW_CONNECTION_INFO
    {
        [NativeTypeName("uint32_t")]
        public uint QuicVersion;

        [NativeTypeName("const QUIC_ADDR *")]
        public sockaddr* LocalAddress;

        [NativeTypeName("const QUIC_ADDR *")]
        public sockaddr* RemoteAddress;

        [NativeTypeName("uint32_t")]
        public uint CryptoBufferLength;

        [NativeTypeName("uint16_t")]
        public ushort ClientAlpnListLength;

        [NativeTypeName("uint16_t")]
        public ushort ServerNameLength;

        [NativeTypeName("uint8_t")]
        public byte NegotiatedAlpnLength;

        [NativeTypeName("const uint8_t *")]
        public byte* CryptoBuffer;

        [NativeTypeName("const uint8_t *")]
        public byte* ClientAlpnList;

        [NativeTypeName("const uint8_t *")]
        public byte* NegotiatedAlpn;

        [NativeTypeName("const char *")]
        public sbyte* ServerName;
    }
}
