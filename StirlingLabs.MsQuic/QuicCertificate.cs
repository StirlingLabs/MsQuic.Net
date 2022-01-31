using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Quic;
using StirlingLabs.Utilities;

namespace StirlingLabs.MsQuic;

public struct QuicCertificate
{
    public QUIC_CERTIFICATE_PKCS12 Pkcs12;
    private GCHandle _pin;
    private bool _ownedMemory;

#if NET5_0_OR_GREATER
    private static void ValidatePkcs12(ReadOnlySpan<byte> bytes, string? password, Action<X509ChainPolicy>? config)
    {
        using var cert = password is null
            ? new X509Certificate2(bytes)
            : new(bytes, password);

#else
    private static void ValidatePkcs12(byte[] bytes, string? password, Action<X509ChainPolicy>? config)
    {
        using var cert = password is null
            ? new X509Certificate2(bytes)
            : new(bytes, password);

#endif

        if (!cert.HasPrivateKey)
            throw new CryptographicException("Missing private key.");

        using var chain = new X509Chain();

        config?.Invoke(chain.ChainPolicy);

        var verified = chain.Build(cert);

        string? statusMessage = null;

        if (!verified)
            statusMessage = string.Join("\n", chain.ChainStatus.Select(s =>
                $"{s.StatusInformation} ({s.Status})"));

        foreach (var elem in chain.ChainElements)
            elem.Certificate.Dispose();

        if (!verified)
            throw new CryptographicException(statusMessage ?? "Certificate validation failed.");
    }

    public unsafe QuicCertificate(Stream certStream, string? password = null, bool skipValidation = false)
    {
        if (certStream is null)
            throw new ArgumentNullException(nameof(certStream));

#if NET5_0_OR_GREATER
        ReadOnlySpan<byte> pkcs12Raw;
#else
        byte[] pkcs12Raw;
#endif

        fixed (QuicCertificate* self = &this)
            pkcs12Raw = StreamToSpan(self, certStream, password);

        if (!skipValidation)
            ValidatePkcs12(pkcs12Raw, password, null);
    }

    public unsafe QuicCertificate(Action<X509ChainPolicy> validationConfig, Stream certStream, string? password = null)
    {
        if (certStream is null)
            throw new ArgumentNullException(nameof(certStream));

#if NET5_0_OR_GREATER
        ReadOnlySpan<byte> pkcs12Raw;
#else
        byte[] pkcs12Raw;
#endif

        fixed (QuicCertificate* self = &this)
            pkcs12Raw = StreamToSpan(self, certStream, password);

        ValidatePkcs12(pkcs12Raw, password, validationConfig);
    }

#if NET5_0_OR_GREATER
    private static unsafe ReadOnlySpan<byte> StreamToSpan(QuicCertificate* cert, Stream certStream, string? password)
#else
    private static unsafe byte[] StreamToSpan(QuicCertificate* cert, Stream certStream, string? password)
#endif
    {
#if NET5_0_OR_GREATER
        ReadOnlySpan<byte> pkcs12Raw;
#else
        byte[] pkcs12Raw;
#endif

        switch (certStream)
        {
            case MemoryStream ms: {
#if NET5_0_OR_GREATER
                if (ms.TryGetBuffer(out var seg))
                {
                    cert->_pin = GCHandle.Alloc(seg.Array, GCHandleType.Pinned);
                    cert->_ownedMemory = false;
                    pkcs12Raw = seg;
                }
                else
                {
                    var a = ms.ToArray();
                    cert->_pin = GCHandle.Alloc(a, GCHandleType.Pinned);
                    cert->_ownedMemory = true;
                    pkcs12Raw = a;
                }
#else
                pkcs12Raw = ms.ToArray();
                cert->_pin = GCHandle.Alloc(pkcs12Raw, GCHandleType.Pinned);
                cert->_ownedMemory = true;
#endif
                break;
            }
            case UnmanagedMemoryStream ums: {
                var l = ums.Length - ums.Position;
#if NET5_0_OR_GREATER
                pkcs12Raw = new(ums.PositionPointer, (int)l);
                cert->_pin = default;
                cert->_ownedMemory = false;
#else
                pkcs12Raw = new byte[l];
                var r = 0;
                do
                    r += ums.Read(pkcs12Raw, 0, (int)l - r);
                while (r < l);
                cert->_pin = GCHandle.Alloc(pkcs12Raw, GCHandleType.Pinned);
                cert->_ownedMemory = true;
#endif
                break;
            }
            case FileStream fs: {
                // TODO: memory mapped? care?
                var l = fs.Length;
                var rawBytes = new byte[l];
                var r = 0;
                do
                    r += fs.Read(rawBytes, 0, (int)l - r);
                while (r < l);
                cert->_pin = GCHandle.Alloc(rawBytes, GCHandleType.Pinned);
                cert->_ownedMemory = true;
                pkcs12Raw = rawBytes;
                break;
            }
            default: {
                using var ms = new MemoryStream();
                certStream.CopyTo(ms);
#if NET5_0_OR_GREATER
                if (ms.TryGetBuffer(out var seg))
                {
                    cert->_pin = GCHandle.Alloc(seg.Array, GCHandleType.Pinned);
                    cert->_ownedMemory = false;
                    pkcs12Raw = seg;
                }
                else
                {
                    var a = ms.ToArray();
                    cert->_pin = GCHandle.Alloc(a, GCHandleType.Pinned);
                    cert->_ownedMemory = true;
                    pkcs12Raw = a;
                }
#else
                pkcs12Raw = ms.ToArray();
                cert->_pin = GCHandle.Alloc(pkcs12Raw, GCHandleType.Pinned);
                cert->_ownedMemory = true;
#endif
                break;
            }
        }

        var utf8Password = Utf8String.Create(password);

        fixed (byte* pRaw = pkcs12Raw)
        {
            cert->Pkcs12.Asn1Blob = pRaw;
            cert->Pkcs12.Asn1BlobLength = (uint)pkcs12Raw.Length;
            cert->Pkcs12.PrivateKeyPassword = utf8Password.Pointer;
        }

        return pkcs12Raw;
    }

    public unsafe QuicCertificate(string thumbprint, StoreName storeName = StoreName.My,
        StoreLocation storeLocation = StoreLocation.CurrentUser)
    {

        using var userCertStore = new X509Store(storeName, storeLocation);

        userCertStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        var certs = userCertStore.Certificates
            .Find(X509FindType.FindByThumbprint, thumbprint, false);

        if (certs.Count == 0)
            throw new KeyNotFoundException("Certificate thumbprint not found in specified certificate store.");

        if (certs.Count > 1)
            throw new KeyNotFoundException("Duplicate certificate thumbprints found in specified certificate store.");

        var cert = certs[0];
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            string noncePassword = null!;
            var noncePasswordUtf8 = SizedUtf8String.Create(32, utf8Span => {
                fixed (sbyte* pUtf8Span = utf8Span)
                {
                    noncePassword = string.Create(32, (IntPtr)pUtf8Span, (a, b) => {
                        var p = (sbyte*)b;
                        for (var i = 0; i < 32; ++i)
                        {
                            var v = RandomNumberGenerator.GetInt32(32, 127);
                            a[i] = (char)v;
                            p[i] = (sbyte)v;
                        }
                    });
                }
            });
#else
            var noncePasswordBuilder = new StringBuilder();
            using var rng = RandomNumberGenerator.Create();
            var byteBuf = new byte[32];
            rng.GetBytes(byteBuf);
            var noncePasswordUtf8 = SizedUtf8String.Create(32, utf8Span => {
                for (var i = 0; i < 32; ++i)
                {
                    var v = (byteBuf[i] & (127 - 32)) + 32;
                    noncePasswordBuilder[i] = (char)v;
                    utf8Span[i] = (sbyte)v;
                }
            });
            var noncePassword = noncePasswordBuilder.ToString();
#endif

            var pkcs12Raw = cert.Export(X509ContentType.Pkcs12, noncePassword);

            _pin = GCHandle.Alloc(pkcs12Raw, GCHandleType.Pinned);
            _ownedMemory = true;

            fixed (byte* pRaw = pkcs12Raw)
            {
                Pkcs12.Asn1Blob = pRaw;
                Pkcs12.Asn1BlobLength = (uint)pkcs12Raw.Length;
                Pkcs12.PrivateKeyPassword = noncePasswordUtf8.Pointer;
            }
        }
    }

    public unsafe void Free()
    {

        var p = Pkcs12.PrivateKeyPassword;
        if (_ownedMemory)
        {
            Unsafe.InitBlock(Pkcs12.Asn1Blob, 0, Pkcs12.Asn1BlobLength);
            if (p != null)
                Unsafe.InitBlock(p, 0, (uint)Utf8String.GetByteStringLength(p, 128));
        }
        Pkcs12.Asn1BlobLength = 0;
        Pkcs12.Asn1Blob = null;
        Pkcs12.PrivateKeyPassword = null;
        if (_pin.IsAllocated)
            _pin.Free();
        Marshal.FreeHGlobal((IntPtr)p);
    }
}
