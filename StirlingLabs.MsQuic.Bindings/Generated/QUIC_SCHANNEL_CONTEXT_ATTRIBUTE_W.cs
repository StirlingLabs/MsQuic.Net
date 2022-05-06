//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace StirlingLabs.MsQuic.Bindings
{
    public unsafe partial struct QUIC_SCHANNEL_CONTEXT_ATTRIBUTE_W
    {
        [NativeTypeName("unsigned long")]
        public uint Attribute;

        public void* Buffer;
    }
}
