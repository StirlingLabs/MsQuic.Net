//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StirlingLabs.MsQuic.Bindings
{
    public unsafe partial struct QUIC_CREDENTIAL_CONFIG
    {
        public QUIC_CREDENTIAL_TYPE Type;

        public QUIC_CREDENTIAL_FLAGS Flags;

        [NativeTypeName("QUIC_CREDENTIAL_CONFIG::(anonymous union at ../inc/msquic.h:276:5)")]
        public _Anonymous_e__Union Anonymous;

        [NativeTypeName("const char *")]
        public sbyte* Principal;

        public void* Reserved;

        [NativeTypeName("QUIC_CREDENTIAL_LOAD_COMPLETE_HANDLER")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, int, void> AsyncHandler;

        public QUIC_ALLOWED_CIPHER_SUITE_FLAGS AllowedCipherSuites;

        public ref QUIC_CERTIFICATE_HASH* CertificateHash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateHash;
            }
        }

        public ref QUIC_CERTIFICATE_HASH_STORE* CertificateHashStore
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateHashStore;
            }
        }

        public ref void* CertificateContext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateContext;
            }
        }

        public ref QUIC_CERTIFICATE_FILE* CertificateFile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateFile;
            }
        }

        public ref QUIC_CERTIFICATE_FILE_PROTECTED* CertificateFileProtected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref this, 1)).Anonymous.CertificateFileProtected;
            }
        }

        public ref QUIC_CERTIFICATE_PKCS12* CertificatePkcs12
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
}
