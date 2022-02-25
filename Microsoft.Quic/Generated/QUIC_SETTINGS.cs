//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Quic
{
    public partial struct QUIC_SETTINGS
    {
        [NativeTypeName("QUIC_SETTINGS::(anonymous union at ../inc/msquic.h:545:5)")]
        public _Anonymous_e__Union Anonymous;

        [NativeTypeName("uint64_t")]
        public ulong MaxBytesPerKey;

        [NativeTypeName("uint64_t")]
        public ulong HandshakeIdleTimeoutMs;

        [NativeTypeName("uint64_t")]
        public ulong IdleTimeoutMs;

        [NativeTypeName("uint64_t")]
        public ulong MtuDiscoverySearchCompleteTimeoutUs;

        [NativeTypeName("uint32_t")]
        public uint TlsClientMaxSendBuffer;

        [NativeTypeName("uint32_t")]
        public uint TlsServerMaxSendBuffer;

        [NativeTypeName("uint32_t")]
        public uint StreamRecvWindowDefault;

        [NativeTypeName("uint32_t")]
        public uint StreamRecvBufferDefault;

        [NativeTypeName("uint32_t")]
        public uint ConnFlowControlWindow;

        [NativeTypeName("uint32_t")]
        public uint MaxWorkerQueueDelayUs;

        [NativeTypeName("uint32_t")]
        public uint MaxStatelessOperations;

        [NativeTypeName("uint32_t")]
        public uint InitialWindowPackets;

        [NativeTypeName("uint32_t")]
        public uint SendIdleTimeoutMs;

        [NativeTypeName("uint32_t")]
        public uint InitialRttMs;

        [NativeTypeName("uint32_t")]
        public uint MaxAckDelayMs;

        [NativeTypeName("uint32_t")]
        public uint DisconnectTimeoutMs;

        [NativeTypeName("uint32_t")]
        public uint KeepAliveIntervalMs;

        [NativeTypeName("uint16_t")]
        public ushort CongestionControlAlgorithm;

        [NativeTypeName("uint16_t")]
        public ushort PeerBidiStreamCount;

        [NativeTypeName("uint16_t")]
        public ushort PeerUnidiStreamCount;

        [NativeTypeName("uint16_t")]
        public ushort MaxBindingStatelessOperations;

        [NativeTypeName("uint16_t")]
        public ushort StatelessOperationExpirationMs;

        [NativeTypeName("uint16_t")]
        public ushort MinimumMtu;

        [NativeTypeName("uint16_t")]
        public ushort MaximumMtu;

        public byte _bitfield;

        [NativeTypeName("uint8_t : 1")]
        public byte SendBufferingEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (byte)(_bitfield & 0x1u);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (byte)((_bitfield & ~0x1u) | (value & 0x1u));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte PacingEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (byte)((_bitfield >> 1) & 0x1u);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte MigrationEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (byte)((_bitfield >> 2) & 0x1u);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte DatagramReceiveEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (byte)((_bitfield >> 3) & 0x1u);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 3)) | ((value & 0x1u) << 3));
            }
        }

        [NativeTypeName("uint8_t : 2")]
        public byte ServerResumptionLevel
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (byte)((_bitfield >> 4) & 0x3u);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (byte)((_bitfield & ~(0x3u << 4)) | ((value & 0x3u) << 4));
            }
        }

        [NativeTypeName("uint8_t : 2")]
        public byte RESERVED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (byte)((_bitfield >> 6) & 0x3u);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _bitfield = (byte)((_bitfield & ~(0x3u << 6)) | ((value & 0x3u) << 6));
            }
        }

        [NativeTypeName("uint8_t")]
        public byte MaxOperationsPerDrain;

        [NativeTypeName("uint8_t")]
        public byte MtuDiscoveryMissingProbeCount;

        public ref ulong IsSetFlags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.IsSetFlags, 1));
            }
        }

        public ref _Anonymous_e__Union._IsSet_e__Struct IsSet
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.IsSet, 1));
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("uint64_t")]
            public ulong IsSetFlags;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:547:9)")]
            public _IsSet_e__Struct IsSet;

            public partial struct _IsSet_e__Struct
            {
                public ulong _bitfield;

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxBytesPerKey
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return _bitfield & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~0x1UL) | (value & 0x1UL);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong HandshakeIdleTimeoutMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 1) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 1)) | ((value & 0x1UL) << 1);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong IdleTimeoutMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 2) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 2)) | ((value & 0x1UL) << 2);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MtuDiscoverySearchCompleteTimeoutUs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 3) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 3)) | ((value & 0x1UL) << 3);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong TlsClientMaxSendBuffer
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 4) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 4)) | ((value & 0x1UL) << 4);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong TlsServerMaxSendBuffer
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 5) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 5)) | ((value & 0x1UL) << 5);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong StreamRecvWindowDefault
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 6) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 6)) | ((value & 0x1UL) << 6);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong StreamRecvBufferDefault
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 7) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 7)) | ((value & 0x1UL) << 7);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong ConnFlowControlWindow
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 8) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 8)) | ((value & 0x1UL) << 8);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxWorkerQueueDelayUs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 9) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 9)) | ((value & 0x1UL) << 9);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxStatelessOperations
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 10) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 10)) | ((value & 0x1UL) << 10);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong InitialWindowPackets
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 11) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 11)) | ((value & 0x1UL) << 11);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong SendIdleTimeoutMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 12) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 12)) | ((value & 0x1UL) << 12);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong InitialRttMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 13) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 13)) | ((value & 0x1UL) << 13);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxAckDelayMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 14) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 14)) | ((value & 0x1UL) << 14);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong DisconnectTimeoutMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 15) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 15)) | ((value & 0x1UL) << 15);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong KeepAliveIntervalMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 16) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 16)) | ((value & 0x1UL) << 16);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong CongestionControlAlgorithm
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 17) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 17)) | ((value & 0x1UL) << 17);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong PeerBidiStreamCount
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 18) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 18)) | ((value & 0x1UL) << 18);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong PeerUnidiStreamCount
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 19) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 19)) | ((value & 0x1UL) << 19);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxBindingStatelessOperations
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 20) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 20)) | ((value & 0x1UL) << 20);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong StatelessOperationExpirationMs
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 21) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 21)) | ((value & 0x1UL) << 21);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MinimumMtu
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 22) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 22)) | ((value & 0x1UL) << 22);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaximumMtu
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 23) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 23)) | ((value & 0x1UL) << 23);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong SendBufferingEnabled
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 24) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 24)) | ((value & 0x1UL) << 24);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong PacingEnabled
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 25) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 25)) | ((value & 0x1UL) << 25);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MigrationEnabled
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 26) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 26)) | ((value & 0x1UL) << 26);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong DatagramReceiveEnabled
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 27) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 27)) | ((value & 0x1UL) << 27);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong ServerResumptionLevel
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 28) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 28)) | ((value & 0x1UL) << 28);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxOperationsPerDrain
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 29) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 29)) | ((value & 0x1UL) << 29);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MtuDiscoveryMissingProbeCount
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 30) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 30)) | ((value & 0x1UL) << 30);
                    }
                }

                [NativeTypeName("uint64_t : 33")]
                public ulong RESERVED
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (_bitfield >> 31) & 0x1UL;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 31)) | ((value & 0x1UL) << 31);
                    }
                }
            }
        }
    }
}
