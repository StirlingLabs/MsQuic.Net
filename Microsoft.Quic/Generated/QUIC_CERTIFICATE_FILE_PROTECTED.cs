//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_CERTIFICATE_FILE_PROTECTED
    {
        [NativeTypeName("const char *")]
        public sbyte* PrivateKeyFile;

        [NativeTypeName("const char *")]
        public sbyte* CertificateFile;

        [NativeTypeName("const char *")]
        public sbyte* PrivateKeyPassword;
    }
}
