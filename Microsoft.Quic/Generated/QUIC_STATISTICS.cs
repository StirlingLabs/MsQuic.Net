//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.CompilerServices;

namespace Microsoft.Quic
{
    public partial struct QUIC_STATISTICS
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

        [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:393:5)")]
        public _Timing_e__Struct Timing;

        [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:398:5)")]
        public _Handshake_e__Struct Handshake;

        [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:403:5)")]
        public _Send_e__Struct Send;

        [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:414:5)")]
        public _Recv_e__Struct Recv;

        [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:424:5)")]
        public _Misc_e__Struct Misc;

        public partial struct _Timing_e__Struct
        {
            [NativeTypeName("uint64_t")]
            public ulong Start;

            [NativeTypeName("uint64_t")]
            public ulong InitialFlightEnd;

            [NativeTypeName("uint64_t")]
            public ulong HandshakeFlightEnd;
        }

        public partial struct _Handshake_e__Struct
        {
            [NativeTypeName("uint32_t")]
            public uint ClientFlight1Bytes;

            [NativeTypeName("uint32_t")]
            public uint ServerFlight1Bytes;

            [NativeTypeName("uint32_t")]
            public uint ClientFlight2Bytes;
        }

        public partial struct _Send_e__Struct
        {
            [NativeTypeName("uint16_t")]
            public ushort PathMtu;

            [NativeTypeName("uint64_t")]
            public ulong TotalPackets;

            [NativeTypeName("uint64_t")]
            public ulong RetransmittablePackets;

            [NativeTypeName("uint64_t")]
            public ulong SuspectedLostPackets;

            [NativeTypeName("uint64_t")]
            public ulong SpuriousLostPackets;

            [NativeTypeName("uint64_t")]
            public ulong TotalBytes;

            [NativeTypeName("uint64_t")]
            public ulong TotalStreamBytes;

            [NativeTypeName("uint32_t")]
            public uint CongestionCount;

            [NativeTypeName("uint32_t")]
            public uint PersistentCongestionCount;
        }

        public partial struct _Recv_e__Struct
        {
            [NativeTypeName("uint64_t")]
            public ulong TotalPackets;

            [NativeTypeName("uint64_t")]
            public ulong ReorderedPackets;

            [NativeTypeName("uint64_t")]
            public ulong DroppedPackets;

            [NativeTypeName("uint64_t")]
            public ulong DuplicatePackets;

            [NativeTypeName("uint64_t")]
            public ulong TotalBytes;

            [NativeTypeName("uint64_t")]
            public ulong TotalStreamBytes;

            [NativeTypeName("uint64_t")]
            public ulong DecryptionFailures;

            [NativeTypeName("uint64_t")]
            public ulong ValidAckFrames;
        }

        public partial struct _Misc_e__Struct
        {
            [NativeTypeName("uint32_t")]
            public uint KeyUpdateCount;
        }
    }
}
