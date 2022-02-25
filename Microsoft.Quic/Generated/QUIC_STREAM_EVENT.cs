//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Quic
{
    public partial struct QUIC_STREAM_EVENT
    {
        public QUIC_STREAM_EVENT_TYPE Type;

        [NativeTypeName("QUIC_STREAM_EVENT::(anonymous union at ../inc/msquic.h:1184:5)")]
        public _Anonymous_e__Union Anonymous;

        public ref _Anonymous_e__Union._START_COMPLETE_e__Struct START_COMPLETE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.START_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._RECEIVE_e__Struct RECEIVE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.RECEIVE, 1));
            }
        }

        public ref _Anonymous_e__Union._SEND_COMPLETE_e__Struct SEND_COMPLETE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SEND_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_SEND_ABORTED_e__Struct PEER_SEND_ABORTED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_SEND_ABORTED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_RECEIVE_ABORTED_e__Struct PEER_RECEIVE_ABORTED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_RECEIVE_ABORTED, 1));
            }
        }

        public ref _Anonymous_e__Union._SEND_SHUTDOWN_COMPLETE_e__Struct SEND_SHUTDOWN_COMPLETE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SEND_SHUTDOWN_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._SHUTDOWN_COMPLETE_e__Struct SHUTDOWN_COMPLETE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SHUTDOWN_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._IDEAL_SEND_BUFFER_SIZE_e__Struct IDEAL_SEND_BUFFER_SIZE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.IDEAL_SEND_BUFFER_SIZE, 1));
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1185:9)")]
            public _START_COMPLETE_e__Struct START_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1191:9)")]
            public _RECEIVE_e__Struct RECEIVE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1200:9)")]
            public _SEND_COMPLETE_e__Struct SEND_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1204:9)")]
            public _PEER_SEND_ABORTED_e__Struct PEER_SEND_ABORTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1207:9)")]
            public _PEER_RECEIVE_ABORTED_e__Struct PEER_RECEIVE_ABORTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1210:9)")]
            public _SEND_SHUTDOWN_COMPLETE_e__Struct SEND_SHUTDOWN_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1213:9)")]
            public _SHUTDOWN_COMPLETE_e__Struct SHUTDOWN_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1218:9)")]
            public _IDEAL_SEND_BUFFER_SIZE_e__Struct IDEAL_SEND_BUFFER_SIZE;

            public partial struct _START_COMPLETE_e__Struct
            {
                [NativeTypeName("HRESULT")]
                public int Status;

                [NativeTypeName("QUIC_UINT62")]
                public ulong ID;

                public byte _bitfield;

                [NativeTypeName("BOOLEAN : 1")]
                public byte PeerAccepted
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

                [NativeTypeName("BOOLEAN : 7")]
                public byte RESERVED
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (byte)((_bitfield >> 1) & 0x7Fu);
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x7Fu << 1)) | ((value & 0x7Fu) << 1));
                    }
                }
            }

            public unsafe partial struct _RECEIVE_e__Struct
            {
                [NativeTypeName("uint64_t")]
                public ulong AbsoluteOffset;

                [NativeTypeName("uint64_t")]
                public ulong TotalBufferLength;

                [NativeTypeName("const QUIC_BUFFER *")]
                public QUIC_BUFFER* Buffers;

                [NativeTypeName("uint32_t")]
                public uint BufferCount;

                public QUIC_RECEIVE_FLAGS Flags;
            }

            public unsafe partial struct _SEND_COMPLETE_e__Struct
            {
                [NativeTypeName("BOOLEAN")]
                public byte Canceled;

                public void* ClientContext;
            }

            public partial struct _PEER_SEND_ABORTED_e__Struct
            {
                [NativeTypeName("QUIC_UINT62")]
                public ulong ErrorCode;
            }

            public partial struct _PEER_RECEIVE_ABORTED_e__Struct
            {
                [NativeTypeName("QUIC_UINT62")]
                public ulong ErrorCode;
            }

            public partial struct _SEND_SHUTDOWN_COMPLETE_e__Struct
            {
                [NativeTypeName("BOOLEAN")]
                public byte Graceful;
            }

            public partial struct _SHUTDOWN_COMPLETE_e__Struct
            {
                [NativeTypeName("BOOLEAN")]
                public byte ConnectionShutdown;

                public byte _bitfield;

                [NativeTypeName("BOOLEAN : 1")]
                public byte AppCloseInProgress
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

                [NativeTypeName("BOOLEAN : 7")]
                public byte RESERVED
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get
                    {
                        return (byte)((_bitfield >> 1) & 0x7Fu);
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x7Fu << 1)) | ((value & 0x7Fu) << 1));
                    }
                }
            }

            public partial struct _IDEAL_SEND_BUFFER_SIZE_e__Struct
            {
                [NativeTypeName("uint64_t")]
                public ulong ByteCount;
            }
        }
    }
}
