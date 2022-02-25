//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_REGISTRATION_CONFIG
    {
        [NativeTypeName("const char *")]
        public sbyte* AppName;

        public QUIC_EXECUTION_PROFILE ExecutionProfile;
    }
}
