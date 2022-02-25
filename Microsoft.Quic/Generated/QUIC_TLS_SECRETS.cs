//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.CompilerServices;

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_TLS_SECRETS
    {
        [NativeTypeName("uint8_t")]
        public byte SecretLength;

        [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:626:5)")]
        public _IsSet_e__Struct IsSet;

        [NativeTypeName("uint8_t [32]")]
        public fixed byte ClientRandom[32];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte ClientEarlyTrafficSecret[64];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte ClientHandshakeTrafficSecret[64];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte ServerHandshakeTrafficSecret[64];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte ClientTrafficSecret0[64];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte ServerTrafficSecret0[64];

        public partial struct _IsSet_e__Struct
        {
            public byte _bitfield;

            [NativeTypeName("uint8_t : 1")]
            public byte ClientRandom
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
            public byte ClientEarlyTrafficSecret
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
            public byte ClientHandshakeTrafficSecret
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
            public byte ServerHandshakeTrafficSecret
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

            [NativeTypeName("uint8_t : 1")]
            public byte ClientTrafficSecret0
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return (byte)((_bitfield >> 4) & 0x1u);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    _bitfield = (byte)((_bitfield & ~(0x1u << 4)) | ((value & 0x1u) << 4));
                }
            }

            [NativeTypeName("uint8_t : 1")]
            public byte ServerTrafficSecret0
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return (byte)((_bitfield >> 5) & 0x1u);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    _bitfield = (byte)((_bitfield & ~(0x1u << 5)) | ((value & 0x1u) << 5));
                }
            }
        }
    }
}
