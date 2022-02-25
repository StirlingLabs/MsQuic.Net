//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System;

namespace Microsoft.Quic
{
    [Flags]
    public enum QUIC_CREDENTIAL_FLAGS
    {
        QUIC_CREDENTIAL_FLAG_NONE = 0x00000000,
        QUIC_CREDENTIAL_FLAG_CLIENT = 0x00000001,
        QUIC_CREDENTIAL_FLAG_LOAD_ASYNCHRONOUS = 0x00000002,
        QUIC_CREDENTIAL_FLAG_NO_CERTIFICATE_VALIDATION = 0x00000004,
        QUIC_CREDENTIAL_FLAG_ENABLE_OCSP = 0x00000008,
        QUIC_CREDENTIAL_FLAG_INDICATE_CERTIFICATE_RECEIVED = 0x00000010,
        QUIC_CREDENTIAL_FLAG_DEFER_CERTIFICATE_VALIDATION = 0x00000020,
        QUIC_CREDENTIAL_FLAG_REQUIRE_CLIENT_AUTHENTICATION = 0x00000040,
        QUIC_CREDENTIAL_FLAG_USE_TLS_BUILTIN_CERTIFICATE_VALIDATION = 0x00000080,
        QUIC_CREDENTIAL_FLAG_REVOCATION_CHECK_END_CERT = 0x00000100,
        QUIC_CREDENTIAL_FLAG_REVOCATION_CHECK_CHAIN = 0x00000200,
        QUIC_CREDENTIAL_FLAG_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x00000400,
        QUIC_CREDENTIAL_FLAG_IGNORE_NO_REVOCATION_CHECK = 0x00000800,
        QUIC_CREDENTIAL_FLAG_IGNORE_REVOCATION_OFFLINE = 0x00001000,
        QUIC_CREDENTIAL_FLAG_SET_ALLOWED_CIPHER_SUITES = 0x00002000,
        QUIC_CREDENTIAL_FLAG_USE_PORTABLE_CERTIFICATES = 0x00004000,
    }
}
