//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Quic
{
    public partial struct QUIC_CONNECTION_EVENT
    {
        public QUIC_CONNECTION_EVENT_TYPE Type;

        [NativeTypeName("QUIC_CONNECTION_EVENT::(anonymous union at ../inc/msquic.h:1003:5)")]
        public _Anonymous_e__Union Anonymous;

        public ref _Anonymous_e__Union._CONNECTED_e__Struct CONNECTED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.CONNECTED, 1));
            }
        }

        public ref _Anonymous_e__Union._SHUTDOWN_INITIATED_BY_TRANSPORT_e__Struct SHUTDOWN_INITIATED_BY_TRANSPORT
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SHUTDOWN_INITIATED_BY_TRANSPORT, 1));
            }
        }

        public ref _Anonymous_e__Union._SHUTDOWN_INITIATED_BY_PEER_e__Struct SHUTDOWN_INITIATED_BY_PEER
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SHUTDOWN_INITIATED_BY_PEER, 1));
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

        public ref _Anonymous_e__Union._LOCAL_ADDRESS_CHANGED_e__Struct LOCAL_ADDRESS_CHANGED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.LOCAL_ADDRESS_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_ADDRESS_CHANGED_e__Struct PEER_ADDRESS_CHANGED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_ADDRESS_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_STREAM_STARTED_e__Struct PEER_STREAM_STARTED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_STREAM_STARTED, 1));
            }
        }

        public ref _Anonymous_e__Union._STREAMS_AVAILABLE_e__Struct STREAMS_AVAILABLE
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.STREAMS_AVAILABLE, 1));
            }
        }

        public ref _Anonymous_e__Union._IDEAL_PROCESSOR_CHANGED_e__Struct IDEAL_PROCESSOR_CHANGED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.IDEAL_PROCESSOR_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._DATAGRAM_STATE_CHANGED_e__Struct DATAGRAM_STATE_CHANGED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.DATAGRAM_STATE_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._DATAGRAM_RECEIVED_e__Struct DATAGRAM_RECEIVED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.DATAGRAM_RECEIVED, 1));
            }
        }

        public ref _Anonymous_e__Union._DATAGRAM_SEND_STATE_CHANGED_e__Struct DATAGRAM_SEND_STATE_CHANGED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.DATAGRAM_SEND_STATE_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._RESUMED_e__Struct RESUMED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.RESUMED, 1));
            }
        }

        public ref _Anonymous_e__Union._RESUMPTION_TICKET_RECEIVED_e__Struct RESUMPTION_TICKET_RECEIVED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.RESUMPTION_TICKET_RECEIVED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_CERTIFICATE_RECEIVED_e__Struct PEER_CERTIFICATE_RECEIVED
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_CERTIFICATE_RECEIVED, 1));
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1004:9)")]
            public _CONNECTED_e__Struct CONNECTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1011:9)")]
            public _SHUTDOWN_INITIATED_BY_TRANSPORT_e__Struct SHUTDOWN_INITIATED_BY_TRANSPORT;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1014:9)")]
            public _SHUTDOWN_INITIATED_BY_PEER_e__Struct SHUTDOWN_INITIATED_BY_PEER;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1017:9)")]
            public _SHUTDOWN_COMPLETE_e__Struct SHUTDOWN_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1022:9)")]
            public _LOCAL_ADDRESS_CHANGED_e__Struct LOCAL_ADDRESS_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1025:9)")]
            public _PEER_ADDRESS_CHANGED_e__Struct PEER_ADDRESS_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1028:9)")]
            public _PEER_STREAM_STARTED_e__Struct PEER_STREAM_STARTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1032:9)")]
            public _STREAMS_AVAILABLE_e__Struct STREAMS_AVAILABLE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1036:9)")]
            public _IDEAL_PROCESSOR_CHANGED_e__Struct IDEAL_PROCESSOR_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1039:9)")]
            public _DATAGRAM_STATE_CHANGED_e__Struct DATAGRAM_STATE_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1043:9)")]
            public _DATAGRAM_RECEIVED_e__Struct DATAGRAM_RECEIVED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1047:9)")]
            public _DATAGRAM_SEND_STATE_CHANGED_e__Struct DATAGRAM_SEND_STATE_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1051:9)")]
            public _RESUMED_e__Struct RESUMED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1055:9)")]
            public _RESUMPTION_TICKET_RECEIVED_e__Struct RESUMPTION_TICKET_RECEIVED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct at ../inc/msquic.h:1061:9)")]
            public _PEER_CERTIFICATE_RECEIVED_e__Struct PEER_CERTIFICATE_RECEIVED;

            public unsafe partial struct _CONNECTED_e__Struct
            {
                [NativeTypeName("BOOLEAN")]
                public byte SessionResumed;

                [NativeTypeName("uint8_t")]
                public byte NegotiatedAlpnLength;

                [NativeTypeName("const uint8_t *")]
                public byte* NegotiatedAlpn;
            }

            public partial struct _SHUTDOWN_INITIATED_BY_TRANSPORT_e__Struct
            {
                [NativeTypeName("HRESULT")]
                public int Status;
            }

            public partial struct _SHUTDOWN_INITIATED_BY_PEER_e__Struct
            {
                [NativeTypeName("QUIC_UINT62")]
                public ulong ErrorCode;
            }

            public partial struct _SHUTDOWN_COMPLETE_e__Struct
            {
                public byte _bitfield;

                [NativeTypeName("BOOLEAN : 1")]
                public byte HandshakeCompleted
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

                [NativeTypeName("BOOLEAN : 1")]
                public byte PeerAcknowledgedShutdown
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

                [NativeTypeName("BOOLEAN : 1")]
                public byte AppCloseInProgress
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
            }

            public unsafe partial struct _LOCAL_ADDRESS_CHANGED_e__Struct
            {
                [NativeTypeName("const QUIC_ADDR *")]
                public sockaddr* Address;
            }

            public unsafe partial struct _PEER_ADDRESS_CHANGED_e__Struct
            {
                [NativeTypeName("const QUIC_ADDR *")]
                public sockaddr* Address;
            }

            public unsafe partial struct _PEER_STREAM_STARTED_e__Struct
            {
                [NativeTypeName("HQUIC")]
                public QUIC_HANDLE* Stream;

                public QUIC_STREAM_OPEN_FLAGS Flags;
            }

            public partial struct _STREAMS_AVAILABLE_e__Struct
            {
                [NativeTypeName("uint16_t")]
                public ushort BidirectionalCount;

                [NativeTypeName("uint16_t")]
                public ushort UnidirectionalCount;
            }

            public partial struct _IDEAL_PROCESSOR_CHANGED_e__Struct
            {
                [NativeTypeName("uint16_t")]
                public ushort IdealProcessor;
            }

            public partial struct _DATAGRAM_STATE_CHANGED_e__Struct
            {
                [NativeTypeName("BOOLEAN")]
                public byte SendEnabled;

                [NativeTypeName("uint16_t")]
                public ushort MaxSendLength;
            }

            public unsafe partial struct _DATAGRAM_RECEIVED_e__Struct
            {
                [NativeTypeName("const QUIC_BUFFER *")]
                public QUIC_BUFFER* Buffer;

                public QUIC_RECEIVE_FLAGS Flags;
            }

            public unsafe partial struct _DATAGRAM_SEND_STATE_CHANGED_e__Struct
            {
                public void* ClientContext;

                public QUIC_DATAGRAM_SEND_STATE State;
            }

            public unsafe partial struct _RESUMED_e__Struct
            {
                [NativeTypeName("uint16_t")]
                public ushort ResumptionStateLength;

                [NativeTypeName("const uint8_t *")]
                public byte* ResumptionState;
            }

            public unsafe partial struct _RESUMPTION_TICKET_RECEIVED_e__Struct
            {
                [NativeTypeName("uint32_t")]
                public uint ResumptionTicketLength;

                [NativeTypeName("const uint8_t *")]
                public byte* ResumptionTicket;
            }

            public unsafe partial struct _PEER_CERTIFICATE_RECEIVED_e__Struct
            {
                [NativeTypeName("QUIC_CERTIFICATE *")]
                public void* Certificate;

                [NativeTypeName("uint32_t")]
                public uint DeferredErrorFlags;

                [NativeTypeName("HRESULT")]
                public int DeferredStatus;

                [NativeTypeName("QUIC_CERTIFICATE_CHAIN *")]
                public void* Chain;
            }
        }
    }
}
