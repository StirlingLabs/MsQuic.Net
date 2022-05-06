//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace StirlingLabs.MsQuic.Bindings
{
    public unsafe partial struct QUIC_BUFFER
    {
        [NativeTypeName("uint32_t")]
        public uint Length;

        [NativeTypeName("uint8_t *")]
        public byte* Buffer;
    }
}
