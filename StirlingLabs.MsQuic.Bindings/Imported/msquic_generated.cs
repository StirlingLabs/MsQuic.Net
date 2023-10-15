#nullable enable
#pragma warning disable IDE0073
//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//
#pragma warning restore IDE0073

#pragma warning disable CS0649

// Polyfill for MemoryMarshal on .NET Standard
#if NETSTANDARD && !NETSTANDARD2_1_OR_GREATER
using MemoryMarshal = StirlingLabs.MsQuic.Bindings.Polyfill.MemoryMarshal;
#else
using MemoryMarshal = System.Runtime.InteropServices.MemoryMarshal;
#endif

using System.Runtime.InteropServices;

namespace StirlingLabs.MsQuic.Bindings
{
    public partial struct QUIC_HANDLE
    {
    }

    public enum QUIC_TLS_PROVIDER
    {
        SCHANNEL = 0x0000,
        OPENSSL = 0x0001,
    }

    public enum QUIC_EXECUTION_PROFILE
    {
        LOW_LATENCY,
        MAX_THROUGHPUT,
        SCAVENGER,
        REAL_TIME,
    }

    public enum QUIC_LOAD_BALANCING_MODE
    {
        DISABLED,
        SERVER_ID_IP,
        SERVER_ID_FIXED,
        COUNT,
    }

    public enum QUIC_TLS_ALERT_CODES
    {
        SUCCESS = 0xFFFF,
        UNEXPECTED_MESSAGE = 10,
        BAD_CERTIFICATE = 42,
        UNSUPPORTED_CERTIFICATE = 43,
        CERTIFICATE_REVOKED = 44,
        CERTIFICATE_EXPIRED = 45,
        CERTIFICATE_UNKNOWN = 46,
        ILLEGAL_PARAMETER = 47,
        UNKNOWN_CA = 48,
        ACCESS_DENIED = 49,
        INSUFFICIENT_SECURITY = 71,
        INTERNAL_ERROR = 80,
        USER_CANCELED = 90,
        CERTIFICATE_REQUIRED = 116,
        MAX = 255,
    }

    public enum QUIC_CREDENTIAL_TYPE
    {
        NONE,
        CERTIFICATE_HASH,
        CERTIFICATE_HASH_STORE,
        CERTIFICATE_CONTEXT,
        CERTIFICATE_FILE,
        CERTIFICATE_FILE_PROTECTED,
        CERTIFICATE_PKCS12,
    }

    [System.Flags]
    public enum QUIC_CREDENTIAL_FLAGS
    {
        NONE = 0x00000000,
        CLIENT = 0x00000001,
        LOAD_ASYNCHRONOUS = 0x00000002,
        NO_CERTIFICATE_VALIDATION = 0x00000004,
        ENABLE_OCSP = 0x00000008,
        INDICATE_CERTIFICATE_RECEIVED = 0x00000010,
        DEFER_CERTIFICATE_VALIDATION = 0x00000020,
        REQUIRE_CLIENT_AUTHENTICATION = 0x00000040,
        USE_TLS_BUILTIN_CERTIFICATE_VALIDATION = 0x00000080,
        REVOCATION_CHECK_END_CERT = 0x00000100,
        REVOCATION_CHECK_CHAIN = 0x00000200,
        REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x00000400,
        IGNORE_NO_REVOCATION_CHECK = 0x00000800,
        IGNORE_REVOCATION_OFFLINE = 0x00001000,
        SET_ALLOWED_CIPHER_SUITES = 0x00002000,
        USE_PORTABLE_CERTIFICATES = 0x00004000,
        USE_SUPPLIED_CREDENTIALS = 0x00008000,
        USE_SYSTEM_MAPPER = 0x00010000,
        CACHE_ONLY_URL_RETRIEVAL = 0x00020000,
        REVOCATION_CHECK_CACHE_ONLY = 0x00040000,
        INPROC_PEER_CERTIFICATE = 0x00080000,
        SET_CA_CERTIFICATE_FILE = 0x00100000,
    }

    [System.Flags]
    public enum QUIC_ALLOWED_CIPHER_SUITE_FLAGS
    {
        NONE = 0x0,
        AES_128_GCM_SHA256 = 0x1,
        AES_256_GCM_SHA384 = 0x2,
        CHACHA20_POLY1305_SHA256 = 0x4,
    }

    [System.Flags]
    public enum QUIC_CERTIFICATE_HASH_STORE_FLAGS
    {
        NONE = 0x0000,
        MACHINE_STORE = 0x0001,
    }

    [System.Flags]
    public enum QUIC_CONNECTION_SHUTDOWN_FLAGS
    {
        NONE = 0x0000,
        SILENT = 0x0001,
    }

    public enum QUIC_SERVER_RESUMPTION_LEVEL
    {
        NO_RESUME,
        RESUME_ONLY,
        RESUME_AND_ZERORTT,
    }

    [System.Flags]
    public enum QUIC_SEND_RESUMPTION_FLAGS
    {
        NONE = 0x0000,
        FINAL = 0x0001,
    }

    public enum QUIC_STREAM_SCHEDULING_SCHEME
    {
        FIFO = 0x0000,
        ROUND_ROBIN = 0x0001,
        COUNT,
    }

    [System.Flags]
    public enum QUIC_STREAM_OPEN_FLAGS
    {
        NONE = 0x0000,
        UNIDIRECTIONAL = 0x0001,
        ZERO_RTT = 0x0002,
        DELAY_ID_FC_UPDATES = 0x0004,
    }

    [System.Flags]
    public enum QUIC_STREAM_START_FLAGS
    {
        NONE = 0x0000,
        IMMEDIATE = 0x0001,
        FAIL_BLOCKED = 0x0002,
        SHUTDOWN_ON_FAIL = 0x0004,
        INDICATE_PEER_ACCEPT = 0x0008,
    }

    [System.Flags]
    public enum QUIC_STREAM_SHUTDOWN_FLAGS
    {
        NONE = 0x0000,
        GRACEFUL = 0x0001,
        ABORT_SEND = 0x0002,
        ABORT_RECEIVE = 0x0004,
        ABORT = 0x0006,
        IMMEDIATE = 0x0008,
        INLINE = 0x0010,
    }

    [System.Flags]
    public enum QUIC_RECEIVE_FLAGS
    {
        NONE = 0x0000,
        ZERO_RTT = 0x0001,
        FIN = 0x0002,
    }

    [System.Flags]
    public enum QUIC_SEND_FLAGS
    {
        NONE = 0x0000,
        ALLOW_0_RTT = 0x0001,
        START = 0x0002,
        FIN = 0x0004,
        DGRAM_PRIORITY = 0x0008,
        DELAY_SEND = 0x0010,
    }

    public enum QUIC_DATAGRAM_SEND_STATE
    {
        UNKNOWN,
        SENT,
        LOST_SUSPECT,
        LOST_DISCARDED,
        ACKNOWLEDGED,
        ACKNOWLEDGED_SPURIOUS,
        CANCELED,
    }

    [System.Flags]
    public enum QUIC_EXECUTION_CONFIG_FLAGS
    {
        NONE = 0x0000,
        QTIP = 0x0001,
        RIO = 0x0002,
    }

    public unsafe partial struct QUIC_EXECUTION_CONFIG
    {
        public QUIC_EXECUTION_CONFIG_FLAGS Flags;

        [NativeTypeName("uint32_t")]
        public uint PollingIdleTimeoutUs;

        [NativeTypeName("uint32_t")]
        public uint ProcessorCount;

        [NativeTypeName("uint16_t [1]")]
        public fixed ushort ProcessorList[1];
    }

    public unsafe partial struct QUIC_REGISTRATION_CONFIG
    {
        [NativeTypeName("const char *")]
        public sbyte* AppName;

        public QUIC_EXECUTION_PROFILE ExecutionProfile;
    }

    public unsafe partial struct QUIC_CERTIFICATE_HASH
    {
        [NativeTypeName("uint8_t [20]")]
        public fixed byte ShaHash[20];
    }

    public unsafe partial struct QUIC_CERTIFICATE_HASH_STORE
    {
        public QUIC_CERTIFICATE_HASH_STORE_FLAGS Flags;

        [NativeTypeName("uint8_t [20]")]
        public fixed byte ShaHash[20];

        [NativeTypeName("char [128]")]
        public fixed sbyte StoreName[128];
    }

    public unsafe partial struct QUIC_CERTIFICATE_FILE
    {
        [NativeTypeName("const char *")]
        public sbyte* PrivateKeyFile;

        [NativeTypeName("const char *")]
        public sbyte* CertificateFile;
    }

    public unsafe partial struct QUIC_CERTIFICATE_FILE_PROTECTED
    {
        [NativeTypeName("const char *")]
        public sbyte* PrivateKeyFile;

        [NativeTypeName("const char *")]
        public sbyte* CertificateFile;

        [NativeTypeName("const char *")]
        public sbyte* PrivateKeyPassword;
    }

    public unsafe partial struct QUIC_CERTIFICATE_PKCS12
    {
        [NativeTypeName("const uint8_t *")]
        public byte* Asn1Blob;

        [NativeTypeName("uint32_t")]
        public uint Asn1BlobLength;

        [NativeTypeName("const char *")]
        public sbyte* PrivateKeyPassword;
    }

    public unsafe partial struct QUIC_CREDENTIAL_CONFIG
    {
        public QUIC_CREDENTIAL_TYPE Type;

        public QUIC_CREDENTIAL_FLAGS Flags;

        [NativeTypeName("QUIC_CREDENTIAL_CONFIG::(anonymous union)")]
        public _Anonymous_e__Union Anonymous;

        [NativeTypeName("const char *")]
        public sbyte* Principal;

        public void* Reserved;

        [NativeTypeName("QUIC_CREDENTIAL_LOAD_COMPLETE_HANDLER")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, int, void> AsyncHandler;

        public QUIC_ALLOWED_CIPHER_SUITE_FLAGS AllowedCipherSuites;

        [NativeTypeName("const char *")]
        public sbyte* CaCertificateFile;

        public ref QUIC_CERTIFICATE_HASH* CertificateHash
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateHash;
            }
        }

        public ref QUIC_CERTIFICATE_HASH_STORE* CertificateHashStore
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateHashStore;
            }
        }

        public ref void* CertificateContext
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateContext;
            }
        }

        public ref QUIC_CERTIFICATE_FILE* CertificateFile
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateFile;
            }
        }

        public ref QUIC_CERTIFICATE_FILE_PROTECTED* CertificateFileProtected
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateFileProtected;
            }
        }

        public ref QUIC_CERTIFICATE_PKCS12* CertificatePkcs12
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificatePkcs12;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            public QUIC_CERTIFICATE_HASH* CertificateHash;

            [FieldOffset(0)]
            public QUIC_CERTIFICATE_HASH_STORE* CertificateHashStore;

            [FieldOffset(0)]
            [NativeTypeName("QUIC_CERTIFICATE *")]
            public void* CertificateContext;

            [FieldOffset(0)]
            public QUIC_CERTIFICATE_FILE* CertificateFile;

            [FieldOffset(0)]
            public QUIC_CERTIFICATE_FILE_PROTECTED* CertificateFileProtected;

            [FieldOffset(0)]
            public QUIC_CERTIFICATE_PKCS12* CertificatePkcs12;
        }
    }

    public unsafe partial struct QUIC_TICKET_KEY_CONFIG
    {
        [NativeTypeName("uint8_t [16]")]
        public fixed byte Id[16];

        [NativeTypeName("uint8_t [64]")]
        public fixed byte Material[64];

        [NativeTypeName("uint8_t")]
        public byte MaterialLength;
    }

    public unsafe partial struct QUIC_BUFFER
    {
        [NativeTypeName("uint32_t")]
        public uint Length;

        [NativeTypeName("uint8_t *")]
        public byte* Buffer;
    }

    public unsafe partial struct QUIC_NEW_CONNECTION_INFO
    {
        [NativeTypeName("uint32_t")]
        public uint QuicVersion;

        [NativeTypeName("const QUIC_ADDR *")]
        public QuicAddr* LocalAddress;

        [NativeTypeName("const QUIC_ADDR *")]
        public QuicAddr* RemoteAddress;

        [NativeTypeName("uint32_t")]
        public uint CryptoBufferLength;

        [NativeTypeName("uint16_t")]
        public ushort ClientAlpnListLength;

        [NativeTypeName("uint16_t")]
        public ushort ServerNameLength;

        [NativeTypeName("uint8_t")]
        public byte NegotiatedAlpnLength;

        [NativeTypeName("const uint8_t *")]
        public byte* CryptoBuffer;

        [NativeTypeName("const uint8_t *")]
        public byte* ClientAlpnList;

        [NativeTypeName("const uint8_t *")]
        public byte* NegotiatedAlpn;

        [NativeTypeName("const char *")]
        public sbyte* ServerName;
    }

    public enum QUIC_TLS_PROTOCOL_VERSION
    {
        UNKNOWN = 0,
        TLS_1_3 = 0x3000,
    }

    public enum QUIC_CIPHER_ALGORITHM
    {
        NONE = 0,
        AES_128 = 0x660E,
        AES_256 = 0x6610,
        CHACHA20 = 0x6612,
    }

    public enum QUIC_HASH_ALGORITHM
    {
        NONE = 0,
        SHA_256 = 0x800C,
        SHA_384 = 0x800D,
    }

    public enum QUIC_KEY_EXCHANGE_ALGORITHM
    {
        NONE = 0,
    }

    public enum QUIC_CIPHER_SUITE
    {
        TLS_AES_128_GCM_SHA256 = 0x1301,
        TLS_AES_256_GCM_SHA384 = 0x1302,
        TLS_CHACHA20_POLY1305_SHA256 = 0x1303,
    }

    public enum QUIC_CONGESTION_CONTROL_ALGORITHM
    {
        CUBIC,
        BBR,
        MAX,
    }

    public partial struct QUIC_HANDSHAKE_INFO
    {
        public QUIC_TLS_PROTOCOL_VERSION TlsProtocolVersion;

        public QUIC_CIPHER_ALGORITHM CipherAlgorithm;

        [NativeTypeName("int32_t")]
        public int CipherStrength;

        public QUIC_HASH_ALGORITHM Hash;

        [NativeTypeName("int32_t")]
        public int HashStrength;

        public QUIC_KEY_EXCHANGE_ALGORITHM KeyExchangeAlgorithm;

        [NativeTypeName("int32_t")]
        public int KeyExchangeStrength;

        public QUIC_CIPHER_SUITE CipherSuite;
    }

    public partial struct QUIC_STATISTICS
    {
        [NativeTypeName("uint64_t")]
        public ulong CorrelationId;

        public uint _bitfield;

        [NativeTypeName("uint32_t : 1")]
        public uint VersionNegotiation
        {
            get
            {
                return _bitfield & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~0x1u) | (value & 0x1u);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint StatelessRetry
        {
            get
            {
                return (_bitfield >> 1) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint ResumptionAttempted
        {
            get
            {
                return (_bitfield >> 2) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint ResumptionSucceeded
        {
            get
            {
                return (_bitfield >> 3) & 0x1u;
            }

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

        [NativeTypeName("struct (anonymous struct)")]
        public _Timing_e__Struct Timing;

        [NativeTypeName("struct (anonymous struct)")]
        public _Handshake_e__Struct Handshake;

        [NativeTypeName("struct (anonymous struct)")]
        public _Send_e__Struct Send;

        [NativeTypeName("struct (anonymous struct)")]
        public _Recv_e__Struct Recv;

        [NativeTypeName("struct (anonymous struct)")]
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

    public partial struct QUIC_STATISTICS_V2
    {
        [NativeTypeName("uint64_t")]
        public ulong CorrelationId;

        public uint _bitfield;

        [NativeTypeName("uint32_t : 1")]
        public uint VersionNegotiation
        {
            get
            {
                return _bitfield & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~0x1u) | (value & 0x1u);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint StatelessRetry
        {
            get
            {
                return (_bitfield >> 1) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint ResumptionAttempted
        {
            get
            {
                return (_bitfield >> 2) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint ResumptionSucceeded
        {
            get
            {
                return (_bitfield >> 3) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 3)) | ((value & 0x1u) << 3);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint GreaseBitNegotiated
        {
            get
            {
                return (_bitfield >> 4) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 4)) | ((value & 0x1u) << 4);
            }
        }

        [NativeTypeName("uint32_t : 1")]
        public uint EcnCapable
        {
            get
            {
                return (_bitfield >> 5) & 0x1u;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x1u << 5)) | ((value & 0x1u) << 5);
            }
        }

        [NativeTypeName("uint32_t : 26")]
        public uint RESERVED
        {
            get
            {
                return (_bitfield >> 6) & 0x3FFFFFFu;
            }

            set
            {
                _bitfield = (_bitfield & ~(0x3FFFFFFu << 6)) | ((value & 0x3FFFFFFu) << 6);
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

        [NativeTypeName("uint32_t")]
        public uint SendCongestionWindow;

        [NativeTypeName("uint32_t")]
        public uint DestCidUpdateCount;

        [NativeTypeName("uint32_t")]
        public uint SendEcnCongestionCount;
    }

    public partial struct QUIC_LISTENER_STATISTICS
    {
        [NativeTypeName("uint64_t")]
        public ulong TotalAcceptedConnections;

        [NativeTypeName("uint64_t")]
        public ulong TotalRejectedConnections;

        [NativeTypeName("uint64_t")]
        public ulong BindingRecvDroppedPackets;
    }

    public enum QUIC_PERFORMANCE_COUNTERS
    {
        CONN_CREATED,
        CONN_HANDSHAKE_FAIL,
        CONN_APP_REJECT,
        CONN_RESUMED,
        CONN_ACTIVE,
        CONN_CONNECTED,
        CONN_PROTOCOL_ERRORS,
        CONN_NO_ALPN,
        STRM_ACTIVE,
        PKTS_SUSPECTED_LOST,
        PKTS_DROPPED,
        PKTS_DECRYPTION_FAIL,
        UDP_RECV,
        UDP_SEND,
        UDP_RECV_BYTES,
        UDP_SEND_BYTES,
        UDP_RECV_EVENTS,
        UDP_SEND_CALLS,
        APP_SEND_BYTES,
        APP_RECV_BYTES,
        CONN_QUEUE_DEPTH,
        CONN_OPER_QUEUE_DEPTH,
        CONN_OPER_QUEUED,
        CONN_OPER_COMPLETED,
        WORK_OPER_QUEUE_DEPTH,
        WORK_OPER_QUEUED,
        WORK_OPER_COMPLETED,
        PATH_VALIDATED,
        PATH_FAILURE,
        SEND_STATELESS_RESET,
        SEND_STATELESS_RETRY,
        MAX,
    }

    public unsafe partial struct QUIC_VERSION_SETTINGS
    {
        [NativeTypeName("const uint32_t *")]
        public uint* AcceptableVersions;

        [NativeTypeName("const uint32_t *")]
        public uint* OfferedVersions;

        [NativeTypeName("const uint32_t *")]
        public uint* FullyDeployedVersions;

        [NativeTypeName("uint32_t")]
        public uint AcceptableVersionsLength;

        [NativeTypeName("uint32_t")]
        public uint OfferedVersionsLength;

        [NativeTypeName("uint32_t")]
        public uint FullyDeployedVersionsLength;
    }

    public partial struct QUIC_GLOBAL_SETTINGS
    {
        [NativeTypeName("QUIC_GLOBAL_SETTINGS::(anonymous union)")]
        public _Anonymous_e__Union Anonymous;

        [NativeTypeName("uint16_t")]
        public ushort RetryMemoryLimit;

        [NativeTypeName("uint16_t")]
        public ushort LoadBalancingMode;

        [NativeTypeName("uint32_t")]
        public uint FixedServerID;

        public ref ulong IsSetFlags
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.IsSetFlags, 1));
            }
        }

        public ref _Anonymous_e__Union._IsSet_e__Struct IsSet
        {
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
            [NativeTypeName("struct (anonymous struct)")]
            public _IsSet_e__Struct IsSet;

            public partial struct _IsSet_e__Struct
            {
                public ulong _bitfield;

                [NativeTypeName("uint64_t : 1")]
                public ulong RetryMemoryLimit
                {
                    get
                    {
                        return _bitfield & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~0x1UL) | (value & 0x1UL);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong LoadBalancingMode
                {
                    get
                    {
                        return (_bitfield >> 1) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 1)) | ((value & 0x1UL) << 1);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong FixedServerID
                {
                    get
                    {
                        return (_bitfield >> 2) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 2)) | ((value & 0x1UL) << 2);
                    }
                }

                [NativeTypeName("uint64_t : 61")]
                public ulong RESERVED
                {
                    get
                    {
                        return (_bitfield >> 3) & 0x1FFFFFFFUL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1FFFFFFFUL << 3)) | ((value & 0x1FFFFFFFUL) << 3);
                    }
                }
            }
        }
    }

    public partial struct QUIC_SETTINGS
    {
        [NativeTypeName("QUIC_SETTINGS::(anonymous union)")]
        public _Anonymous1_e__Union Anonymous1;

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
            get
            {
                return (byte)(_bitfield & 0x1u);
            }

            set
            {
                _bitfield = (byte)((_bitfield & ~0x1u) | (value & 0x1u));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte PacingEnabled
        {
            get
            {
                return (byte)((_bitfield >> 1) & 0x1u);
            }

            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte MigrationEnabled
        {
            get
            {
                return (byte)((_bitfield >> 2) & 0x1u);
            }

            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte DatagramReceiveEnabled
        {
            get
            {
                return (byte)((_bitfield >> 3) & 0x1u);
            }

            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 3)) | ((value & 0x1u) << 3));
            }
        }

        [NativeTypeName("uint8_t : 2")]
        public byte ServerResumptionLevel
        {
            get
            {
                return (byte)((_bitfield >> 4) & 0x3u);
            }

            set
            {
                _bitfield = (byte)((_bitfield & ~(0x3u << 4)) | ((value & 0x3u) << 4));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte GreaseQuicBitEnabled
        {
            get
            {
                return (byte)((_bitfield >> 6) & 0x1u);
            }

            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 6)) | ((value & 0x1u) << 6));
            }
        }

        [NativeTypeName("uint8_t : 1")]
        public byte EcnEnabled
        {
            get
            {
                return (byte)((_bitfield >> 7) & 0x1u);
            }

            set
            {
                _bitfield = (byte)((_bitfield & ~(0x1u << 7)) | ((value & 0x1u) << 7));
            }
        }

        [NativeTypeName("uint8_t")]
        public byte MaxOperationsPerDrain;

        [NativeTypeName("uint8_t")]
        public byte MtuDiscoveryMissingProbeCount;

        [NativeTypeName("uint32_t")]
        public uint DestCidUpdateIdleTimeoutMs;

        [NativeTypeName("QUIC_SETTINGS::(anonymous union)")]
        public _Anonymous2_e__Union Anonymous2;

        public ref ulong IsSetFlags
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous1.IsSetFlags, 1));
            }
        }

        public ref _Anonymous1_e__Union._IsSet_e__Struct IsSet
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous1.IsSet, 1));
            }
        }

        public ref ulong Flags
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous2.Flags, 1));
            }
        }

        public ulong HyStartEnabled
        {
            get
            {
                return Anonymous2.Anonymous.HyStartEnabled;
            }

            set
            {
                Anonymous2.Anonymous.HyStartEnabled = value;
            }
        }

        public ulong ReservedFlags
        {
            get
            {
                return Anonymous2.Anonymous.ReservedFlags;
            }

            set
            {
                Anonymous2.Anonymous.ReservedFlags = value;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous1_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("uint64_t")]
            public ulong IsSetFlags;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _IsSet_e__Struct IsSet;

            public partial struct _IsSet_e__Struct
            {
                public ulong _bitfield;

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxBytesPerKey
                {
                    get
                    {
                        return _bitfield & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~0x1UL) | (value & 0x1UL);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong HandshakeIdleTimeoutMs
                {
                    get
                    {
                        return (_bitfield >> 1) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 1)) | ((value & 0x1UL) << 1);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong IdleTimeoutMs
                {
                    get
                    {
                        return (_bitfield >> 2) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 2)) | ((value & 0x1UL) << 2);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MtuDiscoverySearchCompleteTimeoutUs
                {
                    get
                    {
                        return (_bitfield >> 3) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 3)) | ((value & 0x1UL) << 3);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong TlsClientMaxSendBuffer
                {
                    get
                    {
                        return (_bitfield >> 4) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 4)) | ((value & 0x1UL) << 4);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong TlsServerMaxSendBuffer
                {
                    get
                    {
                        return (_bitfield >> 5) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 5)) | ((value & 0x1UL) << 5);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong StreamRecvWindowDefault
                {
                    get
                    {
                        return (_bitfield >> 6) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 6)) | ((value & 0x1UL) << 6);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong StreamRecvBufferDefault
                {
                    get
                    {
                        return (_bitfield >> 7) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 7)) | ((value & 0x1UL) << 7);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong ConnFlowControlWindow
                {
                    get
                    {
                        return (_bitfield >> 8) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 8)) | ((value & 0x1UL) << 8);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxWorkerQueueDelayUs
                {
                    get
                    {
                        return (_bitfield >> 9) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 9)) | ((value & 0x1UL) << 9);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxStatelessOperations
                {
                    get
                    {
                        return (_bitfield >> 10) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 10)) | ((value & 0x1UL) << 10);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong InitialWindowPackets
                {
                    get
                    {
                        return (_bitfield >> 11) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 11)) | ((value & 0x1UL) << 11);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong SendIdleTimeoutMs
                {
                    get
                    {
                        return (_bitfield >> 12) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 12)) | ((value & 0x1UL) << 12);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong InitialRttMs
                {
                    get
                    {
                        return (_bitfield >> 13) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 13)) | ((value & 0x1UL) << 13);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxAckDelayMs
                {
                    get
                    {
                        return (_bitfield >> 14) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 14)) | ((value & 0x1UL) << 14);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong DisconnectTimeoutMs
                {
                    get
                    {
                        return (_bitfield >> 15) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 15)) | ((value & 0x1UL) << 15);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong KeepAliveIntervalMs
                {
                    get
                    {
                        return (_bitfield >> 16) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 16)) | ((value & 0x1UL) << 16);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong CongestionControlAlgorithm
                {
                    get
                    {
                        return (_bitfield >> 17) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 17)) | ((value & 0x1UL) << 17);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong PeerBidiStreamCount
                {
                    get
                    {
                        return (_bitfield >> 18) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 18)) | ((value & 0x1UL) << 18);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong PeerUnidiStreamCount
                {
                    get
                    {
                        return (_bitfield >> 19) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 19)) | ((value & 0x1UL) << 19);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxBindingStatelessOperations
                {
                    get
                    {
                        return (_bitfield >> 20) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 20)) | ((value & 0x1UL) << 20);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong StatelessOperationExpirationMs
                {
                    get
                    {
                        return (_bitfield >> 21) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 21)) | ((value & 0x1UL) << 21);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MinimumMtu
                {
                    get
                    {
                        return (_bitfield >> 22) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 22)) | ((value & 0x1UL) << 22);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaximumMtu
                {
                    get
                    {
                        return (_bitfield >> 23) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 23)) | ((value & 0x1UL) << 23);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong SendBufferingEnabled
                {
                    get
                    {
                        return (_bitfield >> 24) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 24)) | ((value & 0x1UL) << 24);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong PacingEnabled
                {
                    get
                    {
                        return (_bitfield >> 25) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 25)) | ((value & 0x1UL) << 25);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MigrationEnabled
                {
                    get
                    {
                        return (_bitfield >> 26) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 26)) | ((value & 0x1UL) << 26);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong DatagramReceiveEnabled
                {
                    get
                    {
                        return (_bitfield >> 27) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 27)) | ((value & 0x1UL) << 27);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong ServerResumptionLevel
                {
                    get
                    {
                        return (_bitfield >> 28) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 28)) | ((value & 0x1UL) << 28);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MaxOperationsPerDrain
                {
                    get
                    {
                        return (_bitfield >> 29) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 29)) | ((value & 0x1UL) << 29);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong MtuDiscoveryMissingProbeCount
                {
                    get
                    {
                        return (_bitfield >> 30) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 30)) | ((value & 0x1UL) << 30);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong DestCidUpdateIdleTimeoutMs
                {
                    get
                    {
                        return (_bitfield >> 31) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 31)) | ((value & 0x1UL) << 31);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong GreaseQuicBitEnabled
                {
                    get
                    {
                        return (_bitfield >> 32) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 32)) | ((value & 0x1UL) << 32);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong EcnEnabled
                {
                    get
                    {
                        return (_bitfield >> 33) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 33)) | ((value & 0x1UL) << 33);
                    }
                }

                [NativeTypeName("uint64_t : 1")]
                public ulong HyStartEnabled
                {
                    get
                    {
                        return (_bitfield >> 34) & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1UL << 34)) | ((value & 0x1UL) << 34);
                    }
                }

                [NativeTypeName("uint64_t : 29")]
                public ulong RESERVED
                {
                    get
                    {
                        return (_bitfield >> 35) & 0x1FFFFFFFUL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x1FFFFFFFUL << 35)) | ((value & 0x1FFFFFFFUL) << 35);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous2_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("uint64_t")]
            public ulong Flags;

            [FieldOffset(0)]
            [NativeTypeName("QUIC_SETTINGS::(anonymous struct)")]
            public _Anonymous_e__Struct Anonymous;

            public partial struct _Anonymous_e__Struct
            {
                public ulong _bitfield;

                [NativeTypeName("uint64_t : 1")]
                public ulong HyStartEnabled
                {
                    get
                    {
                        return _bitfield & 0x1UL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~0x1UL) | (value & 0x1UL);
                    }
                }

                [NativeTypeName("uint64_t : 63")]
                public ulong ReservedFlags
                {
                    get
                    {
                        return (_bitfield >> 1) & 0x7FFFFFFFUL;
                    }

                    set
                    {
                        _bitfield = (_bitfield & ~(0x7FFFFFFFUL << 1)) | ((value & 0x7FFFFFFFUL) << 1);
                    }
                }
            }
        }
    }

    public unsafe partial struct QUIC_TLS_SECRETS
    {
        [NativeTypeName("uint8_t")]
        public byte SecretLength;

        [NativeTypeName("struct (anonymous struct)")]
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
                get
                {
                    return (byte)(_bitfield & 0x1u);
                }

                set
                {
                    _bitfield = (byte)((_bitfield & ~0x1u) | (value & 0x1u));
                }
            }

            [NativeTypeName("uint8_t : 1")]
            public byte ClientEarlyTrafficSecret
            {
                get
                {
                    return (byte)((_bitfield >> 1) & 0x1u);
                }

                set
                {
                    _bitfield = (byte)((_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1));
                }
            }

            [NativeTypeName("uint8_t : 1")]
            public byte ClientHandshakeTrafficSecret
            {
                get
                {
                    return (byte)((_bitfield >> 2) & 0x1u);
                }

                set
                {
                    _bitfield = (byte)((_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2));
                }
            }

            [NativeTypeName("uint8_t : 1")]
            public byte ServerHandshakeTrafficSecret
            {
                get
                {
                    return (byte)((_bitfield >> 3) & 0x1u);
                }

                set
                {
                    _bitfield = (byte)((_bitfield & ~(0x1u << 3)) | ((value & 0x1u) << 3));
                }
            }

            [NativeTypeName("uint8_t : 1")]
            public byte ClientTrafficSecret0
            {
                get
                {
                    return (byte)((_bitfield >> 4) & 0x1u);
                }

                set
                {
                    _bitfield = (byte)((_bitfield & ~(0x1u << 4)) | ((value & 0x1u) << 4));
                }
            }

            [NativeTypeName("uint8_t : 1")]
            public byte ServerTrafficSecret0
            {
                get
                {
                    return (byte)((_bitfield >> 5) & 0x1u);
                }

                set
                {
                    _bitfield = (byte)((_bitfield & ~(0x1u << 5)) | ((value & 0x1u) << 5));
                }
            }
        }
    }

    public partial struct QUIC_STREAM_STATISTICS
    {
        [NativeTypeName("uint64_t")]
        public ulong ConnBlockedBySchedulingUs;

        [NativeTypeName("uint64_t")]
        public ulong ConnBlockedByPacingUs;

        [NativeTypeName("uint64_t")]
        public ulong ConnBlockedByAmplificationProtUs;

        [NativeTypeName("uint64_t")]
        public ulong ConnBlockedByCongestionControlUs;

        [NativeTypeName("uint64_t")]
        public ulong ConnBlockedByFlowControlUs;

        [NativeTypeName("uint64_t")]
        public ulong StreamBlockedByIdFlowControlUs;

        [NativeTypeName("uint64_t")]
        public ulong StreamBlockedByFlowControlUs;

        [NativeTypeName("uint64_t")]
        public ulong StreamBlockedByAppUs;
    }

    public unsafe partial struct QUIC_SCHANNEL_CREDENTIAL_ATTRIBUTE_W
    {
        [NativeTypeName("unsigned long")]
        public uint Attribute;

        [NativeTypeName("unsigned long")]
        public uint BufferLength;

        public void* Buffer;
    }

    public unsafe partial struct QUIC_SCHANNEL_CONTEXT_ATTRIBUTE_W
    {
        [NativeTypeName("unsigned long")]
        public uint Attribute;

        public void* Buffer;
    }

    public unsafe partial struct QUIC_SCHANNEL_CONTEXT_ATTRIBUTE_EX_W
    {
        [NativeTypeName("unsigned long")]
        public uint Attribute;

        [NativeTypeName("unsigned long")]
        public uint BufferLength;

        public void* Buffer;
    }

    public enum QUIC_LISTENER_EVENT_TYPE
    {
        NEW_CONNECTION = 0,
        STOP_COMPLETE = 1,
    }

    public partial struct QUIC_LISTENER_EVENT
    {
        public QUIC_LISTENER_EVENT_TYPE Type;

        [NativeTypeName("QUIC_LISTENER_EVENT::(anonymous union)")]
        public _Anonymous_e__Union Anonymous;

        public ref _Anonymous_e__Union._NEW_CONNECTION_e__Struct NEW_CONNECTION
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.NEW_CONNECTION, 1));
            }
        }

        public ref _Anonymous_e__Union._STOP_COMPLETE_e__Struct STOP_COMPLETE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.STOP_COMPLETE, 1));
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _NEW_CONNECTION_e__Struct NEW_CONNECTION;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _STOP_COMPLETE_e__Struct STOP_COMPLETE;

            public unsafe partial struct _NEW_CONNECTION_e__Struct
            {
                [NativeTypeName("const QUIC_NEW_CONNECTION_INFO *")]
                public QUIC_NEW_CONNECTION_INFO* Info;

                [NativeTypeName("HQUIC")]
                public QUIC_HANDLE* Connection;
            }

            public partial struct _STOP_COMPLETE_e__Struct
            {
                public byte _bitfield;

                [NativeTypeName("BOOLEAN : 1")]
                public byte AppCloseInProgress
                {
                    get
                    {
                        return (byte)(_bitfield & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~0x1u) | (value & 0x1u));
                    }
                }

                [NativeTypeName("BOOLEAN : 7")]
                public byte RESERVED
                {
                    get
                    {
                        return (byte)((_bitfield >> 1) & 0x7Fu);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x7Fu << 1)) | ((value & 0x7Fu) << 1));
                    }
                }
            }
        }
    }

    public enum QUIC_CONNECTION_EVENT_TYPE
    {
        CONNECTED = 0,
        SHUTDOWN_INITIATED_BY_TRANSPORT = 1,
        SHUTDOWN_INITIATED_BY_PEER = 2,
        SHUTDOWN_COMPLETE = 3,
        LOCAL_ADDRESS_CHANGED = 4,
        PEER_ADDRESS_CHANGED = 5,
        PEER_STREAM_STARTED = 6,
        STREAMS_AVAILABLE = 7,
        PEER_NEEDS_STREAMS = 8,
        IDEAL_PROCESSOR_CHANGED = 9,
        DATAGRAM_STATE_CHANGED = 10,
        DATAGRAM_RECEIVED = 11,
        DATAGRAM_SEND_STATE_CHANGED = 12,
        RESUMED = 13,
        RESUMPTION_TICKET_RECEIVED = 14,
        PEER_CERTIFICATE_RECEIVED = 15,
    }

    public partial struct QUIC_CONNECTION_EVENT
    {
        public QUIC_CONNECTION_EVENT_TYPE Type;

        [NativeTypeName("QUIC_CONNECTION_EVENT::(anonymous union)")]
        public _Anonymous_e__Union Anonymous;

        public ref _Anonymous_e__Union._CONNECTED_e__Struct CONNECTED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.CONNECTED, 1));
            }
        }

        public ref _Anonymous_e__Union._SHUTDOWN_INITIATED_BY_TRANSPORT_e__Struct SHUTDOWN_INITIATED_BY_TRANSPORT
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SHUTDOWN_INITIATED_BY_TRANSPORT, 1));
            }
        }

        public ref _Anonymous_e__Union._SHUTDOWN_INITIATED_BY_PEER_e__Struct SHUTDOWN_INITIATED_BY_PEER
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SHUTDOWN_INITIATED_BY_PEER, 1));
            }
        }

        public ref _Anonymous_e__Union._SHUTDOWN_COMPLETE_e__Struct SHUTDOWN_COMPLETE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SHUTDOWN_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._LOCAL_ADDRESS_CHANGED_e__Struct LOCAL_ADDRESS_CHANGED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.LOCAL_ADDRESS_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_ADDRESS_CHANGED_e__Struct PEER_ADDRESS_CHANGED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_ADDRESS_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_STREAM_STARTED_e__Struct PEER_STREAM_STARTED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_STREAM_STARTED, 1));
            }
        }

        public ref _Anonymous_e__Union._STREAMS_AVAILABLE_e__Struct STREAMS_AVAILABLE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.STREAMS_AVAILABLE, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_NEEDS_STREAMS_e__Struct PEER_NEEDS_STREAMS
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_NEEDS_STREAMS, 1));
            }
        }

        public ref _Anonymous_e__Union._IDEAL_PROCESSOR_CHANGED_e__Struct IDEAL_PROCESSOR_CHANGED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.IDEAL_PROCESSOR_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._DATAGRAM_STATE_CHANGED_e__Struct DATAGRAM_STATE_CHANGED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.DATAGRAM_STATE_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._DATAGRAM_RECEIVED_e__Struct DATAGRAM_RECEIVED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.DATAGRAM_RECEIVED, 1));
            }
        }

        public ref _Anonymous_e__Union._DATAGRAM_SEND_STATE_CHANGED_e__Struct DATAGRAM_SEND_STATE_CHANGED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.DATAGRAM_SEND_STATE_CHANGED, 1));
            }
        }

        public ref _Anonymous_e__Union._RESUMED_e__Struct RESUMED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.RESUMED, 1));
            }
        }

        public ref _Anonymous_e__Union._RESUMPTION_TICKET_RECEIVED_e__Struct RESUMPTION_TICKET_RECEIVED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.RESUMPTION_TICKET_RECEIVED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_CERTIFICATE_RECEIVED_e__Struct PEER_CERTIFICATE_RECEIVED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_CERTIFICATE_RECEIVED, 1));
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _CONNECTED_e__Struct CONNECTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _SHUTDOWN_INITIATED_BY_TRANSPORT_e__Struct SHUTDOWN_INITIATED_BY_TRANSPORT;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _SHUTDOWN_INITIATED_BY_PEER_e__Struct SHUTDOWN_INITIATED_BY_PEER;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _SHUTDOWN_COMPLETE_e__Struct SHUTDOWN_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _LOCAL_ADDRESS_CHANGED_e__Struct LOCAL_ADDRESS_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _PEER_ADDRESS_CHANGED_e__Struct PEER_ADDRESS_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _PEER_STREAM_STARTED_e__Struct PEER_STREAM_STARTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _STREAMS_AVAILABLE_e__Struct STREAMS_AVAILABLE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _PEER_NEEDS_STREAMS_e__Struct PEER_NEEDS_STREAMS;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _IDEAL_PROCESSOR_CHANGED_e__Struct IDEAL_PROCESSOR_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _DATAGRAM_STATE_CHANGED_e__Struct DATAGRAM_STATE_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _DATAGRAM_RECEIVED_e__Struct DATAGRAM_RECEIVED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _DATAGRAM_SEND_STATE_CHANGED_e__Struct DATAGRAM_SEND_STATE_CHANGED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _RESUMED_e__Struct RESUMED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _RESUMPTION_TICKET_RECEIVED_e__Struct RESUMPTION_TICKET_RECEIVED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
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

                [NativeTypeName("QUIC_UINT62")]
                public ulong ErrorCode;
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
                    get
                    {
                        return (byte)(_bitfield & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~0x1u) | (value & 0x1u));
                    }
                }

                [NativeTypeName("BOOLEAN : 1")]
                public byte PeerAcknowledgedShutdown
                {
                    get
                    {
                        return (byte)((_bitfield >> 1) & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1));
                    }
                }

                [NativeTypeName("BOOLEAN : 1")]
                public byte AppCloseInProgress
                {
                    get
                    {
                        return (byte)((_bitfield >> 2) & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2));
                    }
                }
            }

            public unsafe partial struct _LOCAL_ADDRESS_CHANGED_e__Struct
            {
                [NativeTypeName("const QUIC_ADDR *")]
                public QuicAddr* Address;
            }

            public unsafe partial struct _PEER_ADDRESS_CHANGED_e__Struct
            {
                [NativeTypeName("const QUIC_ADDR *")]
                public QuicAddr* Address;
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

            public partial struct _PEER_NEEDS_STREAMS_e__Struct
            {
                [NativeTypeName("BOOLEAN")]
                public byte Bidirectional;
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

    public enum QUIC_STREAM_EVENT_TYPE
    {
        START_COMPLETE = 0,
        RECEIVE = 1,
        SEND_COMPLETE = 2,
        PEER_SEND_SHUTDOWN = 3,
        PEER_SEND_ABORTED = 4,
        PEER_RECEIVE_ABORTED = 5,
        SEND_SHUTDOWN_COMPLETE = 6,
        SHUTDOWN_COMPLETE = 7,
        IDEAL_SEND_BUFFER_SIZE = 8,
        PEER_ACCEPTED = 9,
    }

    public partial struct QUIC_STREAM_EVENT
    {
        public QUIC_STREAM_EVENT_TYPE Type;

        [NativeTypeName("QUIC_STREAM_EVENT::(anonymous union)")]
        public _Anonymous_e__Union Anonymous;

        public ref _Anonymous_e__Union._START_COMPLETE_e__Struct START_COMPLETE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.START_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._RECEIVE_e__Struct RECEIVE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.RECEIVE, 1));
            }
        }

        public ref _Anonymous_e__Union._SEND_COMPLETE_e__Struct SEND_COMPLETE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SEND_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_SEND_ABORTED_e__Struct PEER_SEND_ABORTED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_SEND_ABORTED, 1));
            }
        }

        public ref _Anonymous_e__Union._PEER_RECEIVE_ABORTED_e__Struct PEER_RECEIVE_ABORTED
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.PEER_RECEIVE_ABORTED, 1));
            }
        }

        public ref _Anonymous_e__Union._SEND_SHUTDOWN_COMPLETE_e__Struct SEND_SHUTDOWN_COMPLETE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SEND_SHUTDOWN_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._SHUTDOWN_COMPLETE_e__Struct SHUTDOWN_COMPLETE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.SHUTDOWN_COMPLETE, 1));
            }
        }

        public ref _Anonymous_e__Union._IDEAL_SEND_BUFFER_SIZE_e__Struct IDEAL_SEND_BUFFER_SIZE
        {
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref Anonymous.IDEAL_SEND_BUFFER_SIZE, 1));
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _START_COMPLETE_e__Struct START_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _RECEIVE_e__Struct RECEIVE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _SEND_COMPLETE_e__Struct SEND_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _PEER_SEND_ABORTED_e__Struct PEER_SEND_ABORTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _PEER_RECEIVE_ABORTED_e__Struct PEER_RECEIVE_ABORTED;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _SEND_SHUTDOWN_COMPLETE_e__Struct SEND_SHUTDOWN_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
            public _SHUTDOWN_COMPLETE_e__Struct SHUTDOWN_COMPLETE;

            [FieldOffset(0)]
            [NativeTypeName("struct (anonymous struct)")]
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
                    get
                    {
                        return (byte)(_bitfield & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~0x1u) | (value & 0x1u));
                    }
                }

                [NativeTypeName("BOOLEAN : 7")]
                public byte RESERVED
                {
                    get
                    {
                        return (byte)((_bitfield >> 1) & 0x7Fu);
                    }

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
                    get
                    {
                        return (byte)(_bitfield & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~0x1u) | (value & 0x1u));
                    }
                }

                [NativeTypeName("BOOLEAN : 1")]
                public byte ConnectionShutdownByApp
                {
                    get
                    {
                        return (byte)((_bitfield >> 1) & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x1u << 1)) | ((value & 0x1u) << 1));
                    }
                }

                [NativeTypeName("BOOLEAN : 1")]
                public byte ConnectionClosedRemotely
                {
                    get
                    {
                        return (byte)((_bitfield >> 2) & 0x1u);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x1u << 2)) | ((value & 0x1u) << 2));
                    }
                }

                [NativeTypeName("BOOLEAN : 5")]
                public byte RESERVED
                {
                    get
                    {
                        return (byte)((_bitfield >> 3) & 0x1Fu);
                    }

                    set
                    {
                        _bitfield = (byte)((_bitfield & ~(0x1Fu << 3)) | ((value & 0x1Fu) << 3));
                    }
                }

                [NativeTypeName("QUIC_UINT62")]
                public ulong ConnectionErrorCode;

                [NativeTypeName("HRESULT")]
                public int ConnectionCloseStatus;
            }

            public partial struct _IDEAL_SEND_BUFFER_SIZE_e__Struct
            {
                [NativeTypeName("uint64_t")]
                public ulong ByteCount;
            }
        }
    }

    public unsafe partial struct QUIC_API_TABLE
    {
        [NativeTypeName("QUIC_SET_CONTEXT_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, void> SetContext;

        [NativeTypeName("QUIC_GET_CONTEXT_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*> GetContext;

        [NativeTypeName("QUIC_SET_CALLBACK_HANDLER_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, void*, void> SetCallbackHandler;

        [NativeTypeName("QUIC_SET_PARAM_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, uint, uint, void*, int> SetParam;

        [NativeTypeName("QUIC_GET_PARAM_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, uint, uint*, void*, int> GetParam;

        [NativeTypeName("QUIC_REGISTRATION_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_REGISTRATION_CONFIG*, QUIC_HANDLE**, int> RegistrationOpen;

        [NativeTypeName("QUIC_REGISTRATION_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> RegistrationClose;

        [NativeTypeName("QUIC_REGISTRATION_SHUTDOWN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_CONNECTION_SHUTDOWN_FLAGS, ulong, void> RegistrationShutdown;

        [NativeTypeName("QUIC_CONFIGURATION_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, QUIC_SETTINGS*, uint, void*, QUIC_HANDLE**, int> ConfigurationOpen;

        [NativeTypeName("QUIC_CONFIGURATION_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ConfigurationClose;

        [NativeTypeName("QUIC_CONFIGURATION_LOAD_CREDENTIAL_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_CREDENTIAL_CONFIG*, int> ConfigurationLoadCredential;

        [NativeTypeName("QUIC_LISTENER_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_LISTENER_EVENT*, int>, void*, QUIC_HANDLE**, int> ListenerOpen;

        [NativeTypeName("QUIC_LISTENER_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ListenerClose;

        [NativeTypeName("QUIC_LISTENER_START_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, QuicAddr*, int> ListenerStart;

        [NativeTypeName("QUIC_LISTENER_STOP_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ListenerStop;

        [NativeTypeName("QUIC_CONNECTION_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>, void*, QUIC_HANDLE**, int> ConnectionOpen;

        [NativeTypeName("QUIC_CONNECTION_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ConnectionClose;

        [NativeTypeName("QUIC_CONNECTION_SHUTDOWN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_CONNECTION_SHUTDOWN_FLAGS, ulong, void> ConnectionShutdown;

        [NativeTypeName("QUIC_CONNECTION_START_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_HANDLE*, ushort, sbyte*, ushort, int> ConnectionStart;

        [NativeTypeName("QUIC_CONNECTION_SET_CONFIGURATION_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_HANDLE*, int> ConnectionSetConfiguration;

        [NativeTypeName("QUIC_CONNECTION_SEND_RESUMPTION_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_SEND_RESUMPTION_FLAGS, ushort, byte*, int> ConnectionSendResumptionTicket;

        [NativeTypeName("QUIC_STREAM_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_STREAM_OPEN_FLAGS, delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, int>, void*, QUIC_HANDLE**, int> StreamOpen;

        [NativeTypeName("QUIC_STREAM_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> StreamClose;

        [NativeTypeName("QUIC_STREAM_START_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_STREAM_START_FLAGS, int> StreamStart;

        [NativeTypeName("QUIC_STREAM_SHUTDOWN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_STREAM_SHUTDOWN_FLAGS, ulong, int> StreamShutdown;

        [NativeTypeName("QUIC_STREAM_SEND_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, QUIC_SEND_FLAGS, void*, int> StreamSend;

        [NativeTypeName("QUIC_STREAM_RECEIVE_COMPLETE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, ulong, void> StreamReceiveComplete;

        [NativeTypeName("QUIC_STREAM_RECEIVE_SET_ENABLED_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, byte, int> StreamReceiveSetEnabled;

        [NativeTypeName("QUIC_DATAGRAM_SEND_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, QUIC_SEND_FLAGS, void*, int> DatagramSend;

        [NativeTypeName("QUIC_CONNECTION_COMP_RESUMPTION_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, byte, int> ConnectionResumptionTicketValidationComplete;

        [NativeTypeName("QUIC_CONNECTION_COMP_CERT_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, byte, QUIC_TLS_ALERT_CODES, int> ConnectionCertificateValidationComplete;
    }

    public static unsafe partial class MsQuic
    {
        [DllImport("msquic-openssl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("HRESULT")]
        public static extern int MsQuicOpenVersion([NativeTypeName("uint32_t")] uint Version, [NativeTypeName("const void **")] void** QuicApi);

        [DllImport("msquic-openssl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void MsQuicClose([NativeTypeName("const void *")] void* QuicApi);

        [NativeTypeName("#define QUIC_MAX_ALPN_LENGTH 255")]
        public const uint QUIC_MAX_ALPN_LENGTH = 255;

        [NativeTypeName("#define QUIC_MAX_SNI_LENGTH 65535")]
        public const uint QUIC_MAX_SNI_LENGTH = 65535;

        [NativeTypeName("#define QUIC_MAX_RESUMPTION_APP_DATA_LENGTH 1000")]
        public const uint QUIC_MAX_RESUMPTION_APP_DATA_LENGTH = 1000;

        [NativeTypeName("#define QUIC_EXECUTION_CONFIG_MIN_SIZE (uint32_t)FIELD_OFFSET(QUIC_EXECUTION_CONFIG, ProcessorList)")]
        public static readonly uint QUIC_EXECUTION_CONFIG_MIN_SIZE = unchecked((uint)((int)(Marshal.OffsetOf<QUIC_EXECUTION_CONFIG>("ProcessorList"))));

        [NativeTypeName("#define QUIC_MAX_TICKET_KEY_COUNT 16")]
        public const uint QUIC_MAX_TICKET_KEY_COUNT = 16;

        [NativeTypeName("#define QUIC_TLS_SECRETS_MAX_SECRET_LEN 64")]
        public const uint QUIC_TLS_SECRETS_MAX_SECRET_LEN = 64;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_GLOBAL 0x01000000")]
        public const uint QUIC_PARAM_PREFIX_GLOBAL = 0x01000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_REGISTRATION 0x02000000")]
        public const uint QUIC_PARAM_PREFIX_REGISTRATION = 0x02000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_CONFIGURATION 0x03000000")]
        public const uint QUIC_PARAM_PREFIX_CONFIGURATION = 0x03000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_LISTENER 0x04000000")]
        public const uint QUIC_PARAM_PREFIX_LISTENER = 0x04000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_CONNECTION 0x05000000")]
        public const uint QUIC_PARAM_PREFIX_CONNECTION = 0x05000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_TLS 0x06000000")]
        public const uint QUIC_PARAM_PREFIX_TLS = 0x06000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_TLS_SCHANNEL 0x07000000")]
        public const uint QUIC_PARAM_PREFIX_TLS_SCHANNEL = 0x07000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_STREAM 0x08000000")]
        public const uint QUIC_PARAM_PREFIX_STREAM = 0x08000000;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT 0x01000000")]
        public const uint QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT = 0x01000000;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_SUPPORTED_VERSIONS 0x01000001")]
        public const uint QUIC_PARAM_GLOBAL_SUPPORTED_VERSIONS = 0x01000001;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_LOAD_BALACING_MODE 0x01000002")]
        public const uint QUIC_PARAM_GLOBAL_LOAD_BALACING_MODE = 0x01000002;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_PERF_COUNTERS 0x01000003")]
        public const uint QUIC_PARAM_GLOBAL_PERF_COUNTERS = 0x01000003;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_LIBRARY_VERSION 0x01000004")]
        public const uint QUIC_PARAM_GLOBAL_LIBRARY_VERSION = 0x01000004;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_SETTINGS 0x01000005")]
        public const uint QUIC_PARAM_GLOBAL_SETTINGS = 0x01000005;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_GLOBAL_SETTINGS 0x01000006")]
        public const uint QUIC_PARAM_GLOBAL_GLOBAL_SETTINGS = 0x01000006;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_VERSION_SETTINGS 0x01000007")]
        public const uint QUIC_PARAM_GLOBAL_VERSION_SETTINGS = 0x01000007;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_LIBRARY_GIT_HASH 0x01000008")]
        public const uint QUIC_PARAM_GLOBAL_LIBRARY_GIT_HASH = 0x01000008;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_EXECUTION_CONFIG 0x01000009")]
        public const uint QUIC_PARAM_GLOBAL_EXECUTION_CONFIG = 0x01000009;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_TLS_PROVIDER 0x0100000A")]
        public const uint QUIC_PARAM_GLOBAL_TLS_PROVIDER = 0x0100000A;

        [NativeTypeName("#define QUIC_PARAM_CONFIGURATION_SETTINGS 0x03000000")]
        public const uint QUIC_PARAM_CONFIGURATION_SETTINGS = 0x03000000;

        [NativeTypeName("#define QUIC_PARAM_CONFIGURATION_TICKET_KEYS 0x03000001")]
        public const uint QUIC_PARAM_CONFIGURATION_TICKET_KEYS = 0x03000001;

        [NativeTypeName("#define QUIC_PARAM_CONFIGURATION_VERSION_SETTINGS 0x03000002")]
        public const uint QUIC_PARAM_CONFIGURATION_VERSION_SETTINGS = 0x03000002;

        [NativeTypeName("#define QUIC_PARAM_CONFIGURATION_SCHANNEL_CREDENTIAL_ATTRIBUTE_W 0x03000003")]
        public const uint QUIC_PARAM_CONFIGURATION_SCHANNEL_CREDENTIAL_ATTRIBUTE_W = 0x03000003;

        [NativeTypeName("#define QUIC_PARAM_LISTENER_LOCAL_ADDRESS 0x04000000")]
        public const uint QUIC_PARAM_LISTENER_LOCAL_ADDRESS = 0x04000000;

        [NativeTypeName("#define QUIC_PARAM_LISTENER_STATS 0x04000001")]
        public const uint QUIC_PARAM_LISTENER_STATS = 0x04000001;

        [NativeTypeName("#define QUIC_PARAM_LISTENER_CIBIR_ID 0x04000002")]
        public const uint QUIC_PARAM_LISTENER_CIBIR_ID = 0x04000002;

        [NativeTypeName("#define QUIC_PARAM_CONN_QUIC_VERSION 0x05000000")]
        public const uint QUIC_PARAM_CONN_QUIC_VERSION = 0x05000000;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_ADDRESS 0x05000001")]
        public const uint QUIC_PARAM_CONN_LOCAL_ADDRESS = 0x05000001;

        [NativeTypeName("#define QUIC_PARAM_CONN_REMOTE_ADDRESS 0x05000002")]
        public const uint QUIC_PARAM_CONN_REMOTE_ADDRESS = 0x05000002;

        [NativeTypeName("#define QUIC_PARAM_CONN_IDEAL_PROCESSOR 0x05000003")]
        public const uint QUIC_PARAM_CONN_IDEAL_PROCESSOR = 0x05000003;

        [NativeTypeName("#define QUIC_PARAM_CONN_SETTINGS 0x05000004")]
        public const uint QUIC_PARAM_CONN_SETTINGS = 0x05000004;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS 0x05000005")]
        public const uint QUIC_PARAM_CONN_STATISTICS = 0x05000005;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS_PLAT 0x05000006")]
        public const uint QUIC_PARAM_CONN_STATISTICS_PLAT = 0x05000006;

        [NativeTypeName("#define QUIC_PARAM_CONN_SHARE_UDP_BINDING 0x05000007")]
        public const uint QUIC_PARAM_CONN_SHARE_UDP_BINDING = 0x05000007;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_BIDI_STREAM_COUNT 0x05000008")]
        public const uint QUIC_PARAM_CONN_LOCAL_BIDI_STREAM_COUNT = 0x05000008;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_UNIDI_STREAM_COUNT 0x05000009")]
        public const uint QUIC_PARAM_CONN_LOCAL_UNIDI_STREAM_COUNT = 0x05000009;

        [NativeTypeName("#define QUIC_PARAM_CONN_MAX_STREAM_IDS 0x0500000A")]
        public const uint QUIC_PARAM_CONN_MAX_STREAM_IDS = 0x0500000A;

        [NativeTypeName("#define QUIC_PARAM_CONN_CLOSE_REASON_PHRASE 0x0500000B")]
        public const uint QUIC_PARAM_CONN_CLOSE_REASON_PHRASE = 0x0500000B;

        [NativeTypeName("#define QUIC_PARAM_CONN_STREAM_SCHEDULING_SCHEME 0x0500000C")]
        public const uint QUIC_PARAM_CONN_STREAM_SCHEDULING_SCHEME = 0x0500000C;

        [NativeTypeName("#define QUIC_PARAM_CONN_DATAGRAM_RECEIVE_ENABLED 0x0500000D")]
        public const uint QUIC_PARAM_CONN_DATAGRAM_RECEIVE_ENABLED = 0x0500000D;

        [NativeTypeName("#define QUIC_PARAM_CONN_DATAGRAM_SEND_ENABLED 0x0500000E")]
        public const uint QUIC_PARAM_CONN_DATAGRAM_SEND_ENABLED = 0x0500000E;

        [NativeTypeName("#define QUIC_PARAM_CONN_DISABLE_1RTT_ENCRYPTION 0x0500000F")]
        public const uint QUIC_PARAM_CONN_DISABLE_1RTT_ENCRYPTION = 0x0500000F;

        [NativeTypeName("#define QUIC_PARAM_CONN_RESUMPTION_TICKET 0x05000010")]
        public const uint QUIC_PARAM_CONN_RESUMPTION_TICKET = 0x05000010;

        [NativeTypeName("#define QUIC_PARAM_CONN_PEER_CERTIFICATE_VALID 0x05000011")]
        public const uint QUIC_PARAM_CONN_PEER_CERTIFICATE_VALID = 0x05000011;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_INTERFACE 0x05000012")]
        public const uint QUIC_PARAM_CONN_LOCAL_INTERFACE = 0x05000012;

        [NativeTypeName("#define QUIC_PARAM_CONN_TLS_SECRETS 0x05000013")]
        public const uint QUIC_PARAM_CONN_TLS_SECRETS = 0x05000013;

        [NativeTypeName("#define QUIC_PARAM_CONN_VERSION_SETTINGS 0x05000014")]
        public const uint QUIC_PARAM_CONN_VERSION_SETTINGS = 0x05000014;

        [NativeTypeName("#define QUIC_PARAM_CONN_CIBIR_ID 0x05000015")]
        public const uint QUIC_PARAM_CONN_CIBIR_ID = 0x05000015;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS_V2 0x05000016")]
        public const uint QUIC_PARAM_CONN_STATISTICS_V2 = 0x05000016;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS_V2_PLAT 0x05000017")]
        public const uint QUIC_PARAM_CONN_STATISTICS_V2_PLAT = 0x05000017;

        [NativeTypeName("#define QUIC_PARAM_TLS_HANDSHAKE_INFO 0x06000000")]
        public const uint QUIC_PARAM_TLS_HANDSHAKE_INFO = 0x06000000;

        [NativeTypeName("#define QUIC_PARAM_TLS_NEGOTIATED_ALPN 0x06000001")]
        public const uint QUIC_PARAM_TLS_NEGOTIATED_ALPN = 0x06000001;

        [NativeTypeName("#define QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_W 0x07000000")]
        public const uint QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_W = 0x07000000;

        [NativeTypeName("#define QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_EX_W 0x07000001")]
        public const uint QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_EX_W = 0x07000001;

        [NativeTypeName("#define QUIC_PARAM_TLS_SCHANNEL_SECURITY_CONTEXT_TOKEN 0x07000002")]
        public const uint QUIC_PARAM_TLS_SCHANNEL_SECURITY_CONTEXT_TOKEN = 0x07000002;

        [NativeTypeName("#define QUIC_PARAM_STREAM_ID 0x08000000")]
        public const uint QUIC_PARAM_STREAM_ID = 0x08000000;

        [NativeTypeName("#define QUIC_PARAM_STREAM_0RTT_LENGTH 0x08000001")]
        public const uint QUIC_PARAM_STREAM_0RTT_LENGTH = 0x08000001;

        [NativeTypeName("#define QUIC_PARAM_STREAM_IDEAL_SEND_BUFFER_SIZE 0x08000002")]
        public const uint QUIC_PARAM_STREAM_IDEAL_SEND_BUFFER_SIZE = 0x08000002;

        [NativeTypeName("#define QUIC_PARAM_STREAM_PRIORITY 0x08000003")]
        public const uint QUIC_PARAM_STREAM_PRIORITY = 0x08000003;

        [NativeTypeName("#define QUIC_PARAM_STREAM_STATISTICS 0X08000004")]
        public const uint QUIC_PARAM_STREAM_STATISTICS = 0X08000004;

        [NativeTypeName("#define QUIC_API_VERSION_2 2")]
        public const uint QUIC_API_VERSION_2 = 2;
    }
}
