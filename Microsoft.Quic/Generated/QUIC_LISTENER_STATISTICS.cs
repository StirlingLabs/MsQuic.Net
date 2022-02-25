//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public partial struct QUIC_LISTENER_STATISTICS
    {
        [NativeTypeName("uint64_t")]
        public ulong TotalAcceptedConnections;

        [NativeTypeName("uint64_t")]
        public ulong TotalRejectedConnections;

        [NativeTypeName("uint64_t")]
        public ulong BindingRecvDroppedPackets;
    }
}
