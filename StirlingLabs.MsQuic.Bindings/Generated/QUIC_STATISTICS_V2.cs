//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.CompilerServices;

namespace StirlingLabs.MsQuic.Bindings
{
    public partial struct QUIC_STATISTICS_V2
    {
        [NativeTypeName("uint64_t")]
        public ulong CorrelationId;

        public uint _bitfield;

        [NativeTypeName("uint32_t : 1")]
        public uint VersionNegotiation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _bitfield & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~0x1u) | (value & 0x1u);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint StatelessRetry
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (_bitfield >> 1) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint ResumptionAttempted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (_bitfield >> 2) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint ResumptionSucceeded
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (_bitfield >> 3) & 0x1u;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (_bitfield & ~(0x1u << 3)) | ((value & 0x1u) << 3);
            }
        }

        [NativeTypeName("uint32_t")]
        public uint Rtt;

        [NativeTypeName("uint32_t")]
        public uint MinRtt;

        [NativeTypeName("uint32_t")]
        public uint MaxRtt;

        [NativeTypeName("uint64_t")]
        public ulong TimingStart;

        [NativeTypeName("uint64_t")]
        public ulong TimingInitialFlightEnd;

        [NativeTypeName("uint64_t")]
        public ulong TimingHandshakeFlightEnd;

        [NativeTypeName("uint32_t")]
        public uint HandshakeClientFlight1Bytes;

        [NativeTypeName("uint32_t")]
        public uint HandshakeServerFlight1Bytes;

        [NativeTypeName("uint32_t")]
        public uint HandshakeClientFlight2Bytes;

        [NativeTypeName("uint16_t")]
        public ushort SendPathMtu;

        [NativeTypeName("uint64_t")]
        public ulong SendTotalPackets;

        [NativeTypeName("uint64_t")]
        public ulong SendRetransmittablePackets;

        [NativeTypeName("uint64_t")]
        public ulong SendSuspectedLostPackets;

        [NativeTypeName("uint64_t")]
        public ulong SendSpuriousLostPackets;

        [NativeTypeName("uint64_t")]
        public ulong SendTotalBytes;

        [NativeTypeName("uint64_t")]
        public ulong SendTotalStreamBytes;

        [NativeTypeName("uint32_t")]
        public uint SendCongestionCount;

        [NativeTypeName("uint32_t")]
        public uint SendPersistentCongestionCount;

        [NativeTypeName("uint64_t")]
        public ulong RecvTotalPackets;

        [NativeTypeName("uint64_t")]
        public ulong RecvReorderedPackets;

        [NativeTypeName("uint64_t")]
        public ulong RecvDroppedPackets;

        [NativeTypeName("uint64_t")]
        public ulong RecvDuplicatePackets;

        [NativeTypeName("uint64_t")]
        public ulong RecvTotalBytes;

        [NativeTypeName("uint64_t")]
        public ulong RecvTotalStreamBytes;

        [NativeTypeName("uint64_t")]
        public ulong RecvDecryptionFailures;

        [NativeTypeName("uint64_t")]
        public ulong RecvValidAckFrames;

        [NativeTypeName("uint32_t")]
        public uint KeyUpdateCount;
    }
}
