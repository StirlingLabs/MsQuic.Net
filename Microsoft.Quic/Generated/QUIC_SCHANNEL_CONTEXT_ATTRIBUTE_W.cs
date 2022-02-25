//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_SCHANNEL_CONTEXT_ATTRIBUTE_W
    {
        [NativeTypeName("unsigned long")]
        public uint Attribute;

        public void* Buffer;
    }
}
