//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_BUFFER
    {
        [NativeTypeName("uint32_t")]
        public uint Length;

        [NativeTypeName("uint8_t *")]
        public byte* Buffer;
    }
}
