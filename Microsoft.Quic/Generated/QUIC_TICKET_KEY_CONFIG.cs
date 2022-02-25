//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_TICKET_KEY_CONFIG
    {
        [NativeTypeName("uint8_t [16]")]
        public fixed byte Id[16];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte Material[64];

        [NativeTypeName("uint8_t")]
        public byte MaterialLength;
    }
}
