//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.InteropServices;

namespace Microsoft.Quic
{
    public static unsafe partial class MsQuic
    {
        [DllImport("msquic-openssl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("HRESULT")]
        public static extern int MsQuicOpenVersion([NativeTypeName("uint32_t")] uint Version, [NativeTypeName("const void **")] void** QuicApi);

        [DllImport("msquic-openssl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void MsQuicClose([NativeTypeName("const void *")] void* QuicApi);

        [NativeTypeName("#define QUIC_MAX_ALPN_LENGTH 255")]
        public const int QUIC_MAX_ALPN_LENGTH = 255;

        [NativeTypeName("#define QUIC_MAX_SNI_LENGTH 65535")]
        public const int QUIC_MAX_SNI_LENGTH = 65535;

        [NativeTypeName("#define QUIC_MAX_RESUMPTION_APP_DATA_LENGTH 1000")]
        public const int QUIC_MAX_RESUMPTION_APP_DATA_LENGTH = 1000;

        [NativeTypeName("#define QUIC_MAX_TICKET_KEY_COUNT 16")]
        public const int QUIC_MAX_TICKET_KEY_COUNT = 16;

        [NativeTypeName("#define QUIC_TLS_SECRETS_MAX_SECRET_LEN 64")]
        public const int QUIC_TLS_SECRETS_MAX_SECRET_LEN = 64;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_GLOBAL 0x01000000")]
        public const int QUIC_PARAM_PREFIX_GLOBAL = 0x01000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_REGISTRATION 0x02000000")]
        public const int QUIC_PARAM_PREFIX_REGISTRATION = 0x02000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_CONFIGURATION 0x03000000")]
        public const int QUIC_PARAM_PREFIX_CONFIGURATION = 0x03000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_LISTENER 0x04000000")]
        public const int QUIC_PARAM_PREFIX_LISTENER = 0x04000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_CONNECTION 0x05000000")]
        public const int QUIC_PARAM_PREFIX_CONNECTION = 0x05000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_TLS 0x06000000")]
        public const int QUIC_PARAM_PREFIX_TLS = 0x06000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_TLS_SCHANNEL 0x07000000")]
        public const int QUIC_PARAM_PREFIX_TLS_SCHANNEL = 0x07000000;

        [NativeTypeName("#define QUIC_PARAM_PREFIX_STREAM 0x08000000")]
        public const int QUIC_PARAM_PREFIX_STREAM = 0x08000000;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT 0x01000000")]
        public const int QUIC_PARAM_GLOBAL_RETRY_MEMORY_PERCENT = 0x01000000;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_SUPPORTED_VERSIONS 0x01000001")]
        public const int QUIC_PARAM_GLOBAL_SUPPORTED_VERSIONS = 0x01000001;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_LOAD_BALACING_MODE 0x01000002")]
        public const int QUIC_PARAM_GLOBAL_LOAD_BALACING_MODE = 0x01000002;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_PERF_COUNTERS 0x01000003")]
        public const int QUIC_PARAM_GLOBAL_PERF_COUNTERS = 0x01000003;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_LIBRARY_VERSION 0x01000004")]
        public const int QUIC_PARAM_GLOBAL_LIBRARY_VERSION = 0x01000004;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_SETTINGS 0x01000005")]
        public const int QUIC_PARAM_GLOBAL_SETTINGS = 0x01000005;

        [NativeTypeName("#define QUIC_PARAM_GLOBAL_GLOBAL_SETTINGS 0x01000006")]
        public const int QUIC_PARAM_GLOBAL_GLOBAL_SETTINGS = 0x01000006;

        [NativeTypeName("#define QUIC_PARAM_CONFIGURATION_SETTINGS 0x03000000")]
        public const int QUIC_PARAM_CONFIGURATION_SETTINGS = 0x03000000;

        [NativeTypeName("#define QUIC_PARAM_CONFIGURATION_TICKET_KEYS 0x03000001")]
        public const int QUIC_PARAM_CONFIGURATION_TICKET_KEYS = 0x03000001;

        [NativeTypeName("#define QUIC_PARAM_LISTENER_LOCAL_ADDRESS 0x04000000")]
        public const int QUIC_PARAM_LISTENER_LOCAL_ADDRESS = 0x04000000;

        [NativeTypeName("#define QUIC_PARAM_LISTENER_STATS 0x04000001")]
        public const int QUIC_PARAM_LISTENER_STATS = 0x04000001;

        [NativeTypeName("#define QUIC_PARAM_LISTENER_CID_PREFIX 0x04000002")]
        public const int QUIC_PARAM_LISTENER_CID_PREFIX = 0x04000002;

        [NativeTypeName("#define QUIC_PARAM_CONN_QUIC_VERSION 0x05000000")]
        public const int QUIC_PARAM_CONN_QUIC_VERSION = 0x05000000;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_ADDRESS 0x05000001")]
        public const int QUIC_PARAM_CONN_LOCAL_ADDRESS = 0x05000001;

        [NativeTypeName("#define QUIC_PARAM_CONN_REMOTE_ADDRESS 0x05000002")]
        public const int QUIC_PARAM_CONN_REMOTE_ADDRESS = 0x05000002;

        [NativeTypeName("#define QUIC_PARAM_CONN_IDEAL_PROCESSOR 0x05000003")]
        public const int QUIC_PARAM_CONN_IDEAL_PROCESSOR = 0x05000003;

        [NativeTypeName("#define QUIC_PARAM_CONN_SETTINGS 0x05000004")]
        public const int QUIC_PARAM_CONN_SETTINGS = 0x05000004;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS 0x05000005")]
        public const int QUIC_PARAM_CONN_STATISTICS = 0x05000005;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS_PLAT 0x05000006")]
        public const int QUIC_PARAM_CONN_STATISTICS_PLAT = 0x05000006;

        [NativeTypeName("#define QUIC_PARAM_CONN_SHARE_UDP_BINDING 0x05000007")]
        public const int QUIC_PARAM_CONN_SHARE_UDP_BINDING = 0x05000007;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_BIDI_STREAM_COUNT 0x05000008")]
        public const int QUIC_PARAM_CONN_LOCAL_BIDI_STREAM_COUNT = 0x05000008;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_UNIDI_STREAM_COUNT 0x05000009")]
        public const int QUIC_PARAM_CONN_LOCAL_UNIDI_STREAM_COUNT = 0x05000009;

        [NativeTypeName("#define QUIC_PARAM_CONN_MAX_STREAM_IDS 0x0500000A")]
        public const int QUIC_PARAM_CONN_MAX_STREAM_IDS = 0x0500000A;

        [NativeTypeName("#define QUIC_PARAM_CONN_CLOSE_REASON_PHRASE 0x0500000B")]
        public const int QUIC_PARAM_CONN_CLOSE_REASON_PHRASE = 0x0500000B;

        [NativeTypeName("#define QUIC_PARAM_CONN_STREAM_SCHEDULING_SCHEME 0x0500000C")]
        public const int QUIC_PARAM_CONN_STREAM_SCHEDULING_SCHEME = 0x0500000C;

        [NativeTypeName("#define QUIC_PARAM_CONN_DATAGRAM_RECEIVE_ENABLED 0x0500000D")]
        public const int QUIC_PARAM_CONN_DATAGRAM_RECEIVE_ENABLED = 0x0500000D;

        [NativeTypeName("#define QUIC_PARAM_CONN_DATAGRAM_SEND_ENABLED 0x0500000E")]
        public const int QUIC_PARAM_CONN_DATAGRAM_SEND_ENABLED = 0x0500000E;

        [NativeTypeName("#define QUIC_PARAM_CONN_DISABLE_1RTT_ENCRYPTION 0x0500000F")]
        public const int QUIC_PARAM_CONN_DISABLE_1RTT_ENCRYPTION = 0x0500000F;

        [NativeTypeName("#define QUIC_PARAM_CONN_RESUMPTION_TICKET 0x05000010")]
        public const int QUIC_PARAM_CONN_RESUMPTION_TICKET = 0x05000010;

        [NativeTypeName("#define QUIC_PARAM_CONN_PEER_CERTIFICATE_VALID 0x05000011")]
        public const int QUIC_PARAM_CONN_PEER_CERTIFICATE_VALID = 0x05000011;

        [NativeTypeName("#define QUIC_PARAM_CONN_LOCAL_INTERFACE 0x05000012")]
        public const int QUIC_PARAM_CONN_LOCAL_INTERFACE = 0x05000012;

        [NativeTypeName("#define QUIC_PARAM_CONN_TLS_SECRETS 0x05000013")]
        public const int QUIC_PARAM_CONN_TLS_SECRETS = 0x05000013;

        [NativeTypeName("#define QUIC_PARAM_CONN_INITIAL_DCID_PREFIX 0x05000015")]
        public const int QUIC_PARAM_CONN_INITIAL_DCID_PREFIX = 0x05000015;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS_V2 0x05000016")]
        public const int QUIC_PARAM_CONN_STATISTICS_V2 = 0x05000016;

        [NativeTypeName("#define QUIC_PARAM_CONN_STATISTICS_V2_PLAT 0x05000017")]
        public const int QUIC_PARAM_CONN_STATISTICS_V2_PLAT = 0x05000017;

        [NativeTypeName("#define QUIC_PARAM_TLS_HANDSHAKE_INFO 0x06000000")]
        public const int QUIC_PARAM_TLS_HANDSHAKE_INFO = 0x06000000;

        [NativeTypeName("#define QUIC_PARAM_TLS_NEGOTIATED_ALPN 0x06000001")]
        public const int QUIC_PARAM_TLS_NEGOTIATED_ALPN = 0x06000001;

        [NativeTypeName("#define QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_W 0x07000000")]
        public const int QUIC_PARAM_TLS_SCHANNEL_CONTEXT_ATTRIBUTE_W = 0x07000000;

        [NativeTypeName("#define QUIC_PARAM_STREAM_ID 0x08000000")]
        public const int QUIC_PARAM_STREAM_ID = 0x08000000;

        [NativeTypeName("#define QUIC_PARAM_STREAM_0RTT_LENGTH 0x08000001")]
        public const int QUIC_PARAM_STREAM_0RTT_LENGTH = 0x08000001;

        [NativeTypeName("#define QUIC_PARAM_STREAM_IDEAL_SEND_BUFFER_SIZE 0x08000002")]
        public const int QUIC_PARAM_STREAM_IDEAL_SEND_BUFFER_SIZE = 0x08000002;

        [NativeTypeName("#define QUIC_PARAM_STREAM_PRIORITY 0x08000003")]
        public const int QUIC_PARAM_STREAM_PRIORITY = 0x08000003;

        [NativeTypeName("#define QUIC_API_VERSION_2 2")]
        public const int QUIC_API_VERSION_2 = 2;
    }
}
