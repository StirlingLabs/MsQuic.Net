//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_CERTIFICATE_HASH_STORE
    {
        public QUIC_CERTIFICATE_HASH_STORE_FLAGS Flags;

        [NativeTypeName("uint8_t [20]")]
        public fixed byte ShaHash[20];

        [NativeTypeName("char [128]")]
        public fixed sbyte StoreName[128];
    }
}
