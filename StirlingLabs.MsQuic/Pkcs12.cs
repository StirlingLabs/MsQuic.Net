# if !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Asn1;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using StirlingLabs.Utilities;
using static System.Formats.Asn1.AsnDecoder;
using static System.Formats.Asn1.AsnDecoder;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

#pragma warning disable SYSLIB0026

namespace StirlingLabs.MsQuic;

[PublicAPI]
[SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global")]
public class Pkcs12 : IEquatable<Pkcs12>, IComparable<Pkcs12>
{
    private readonly byte[] _rawData;
    private readonly Pkcs12Info _info;
    private readonly SecureString? _pw;
    private ImmutableArray<X509Certificate>? _bcCerts;
    private ImmutableArray<X509Certificate2>? _msCerts;

    public Pkcs12(byte[] rawData, bool owner)
        : this(rawData, (SecureString?)null, owner) { }

    public Pkcs12(byte[] rawData, SecureString? password, bool owner)
    {
        if (rawData is null)
            throw new ArgumentNullException(nameof(rawData));

        if (!owner)
        {
            var copy = new byte[rawData.Length];
            rawData.CopyTo(copy, 0);
            _rawData = copy;
        }
        else
            _rawData = rawData;

        _info = Pkcs12Info.Decode(_rawData, out _);
        _pw = password is not null ? SecureCopy(password) : null;
    }

    public Pkcs12(byte[] rawData, string? password, bool owner)
        : this(rawData, owner)
    {
        if (rawData is null)
            throw new ArgumentNullException(nameof(rawData));
        if (!owner)
        {
            var copy = new byte[rawData.Length];
            rawData.CopyTo(copy, 0);
            _rawData = copy;
        }
        else
            _rawData = rawData;

        _info = Pkcs12Info.Decode(_rawData, out _);
        _pw = password is not null ? SecureCopy(password) : null;
    }

    public Pkcs12(byte[] rawData)
        : this(rawData, false) { }

    public Pkcs12(byte[] rawData, SecureString password)
        : this(rawData, password, false) { }

    public Pkcs12(byte[] rawData, string password)
        : this(rawData, password, false) { }


    public Pkcs12(ReadOnlyMemory<byte> rawData)
        : this(rawData.ToArray(), true) { }

    public Pkcs12(ReadOnlyMemory<byte> rawData, SecureString? password)
        : this(rawData.ToArray(), password, true) { }

    public Pkcs12(ReadOnlyMemory<byte> rawData, string? password)
        : this(rawData.ToArray(), password, true) { }


    public Pkcs12(ReadOnlySpan<byte> rawData)
        : this(rawData.ToArray(), true) { }

    public Pkcs12(ReadOnlySpan<byte> rawData, SecureString? password)
        : this(rawData.ToArray(), password, true) { }

    public Pkcs12(ReadOnlySpan<byte> rawData, string? password)
        : this(rawData.ToArray(), password, true) { }

    public Pkcs12(string fileName)
        : this(fileName, (SecureString?)null) { }

    public unsafe Pkcs12(string fileName, SecureString? password)
    {
        if (fileName is null) throw new ArgumentNullException(nameof(fileName));
        using var fs = File.OpenRead(fileName);
        var l = fs.Length;
        using var mmf = MemoryMappedFile.CreateFromFile(
            fs,
            null,
            l,
            MemoryMappedFileAccess.Read,
            HandleInheritability.None,
            true
        );
        using var view = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        byte* p = null;
        view.SafeMemoryMappedViewHandle.AcquirePointer(ref p);
        using var ums = new UnmanagedMemoryStream(p, l);
        var b = new byte[l];
        var read = ums.Read(b);
        if (read != l) throw new NotImplementedException();
        _rawData = b;
        _info = Pkcs12Info.Decode(b, out _, true);
        _pw = password is not null ? SecureCopy(password) : null;
    }
    public unsafe Pkcs12(string fileName, string? password)
    {
        if (fileName is null) throw new ArgumentNullException(nameof(fileName));
        using var fs = File.OpenRead(fileName);
        var l = fs.Length;
        using var mmf = MemoryMappedFile.CreateFromFile(
            fs,
            null,
            l,
            MemoryMappedFileAccess.Read,
            HandleInheritability.None,
            true
        );
        using var view = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
        byte* p = null;
        view.SafeMemoryMappedViewHandle.AcquirePointer(ref p);
        using var ums = new UnmanagedMemoryStream(p, l);
        var b = new byte[l];
        var read = ums.Read(b);
        if (read != l) throw new NotImplementedException();
        _rawData = b;
        _info = Pkcs12Info.Decode(b, out _, true);
        _pw = password is not null ? SecureCopy(password) : null;
    }

    private static unsafe SecureString? SecureCopy(string password)
    {
        fixed (char* pChars = password)
        {
            var s = new SecureString(pChars, password.Length);
            s.MakeReadOnly();
            return s;
        }
    }

    private static SecureString SecureCopy(SecureString password)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));
        var s = password.Copy();
        s.MakeReadOnly();
        return s;
    }

    private static unsafe void ReadSecureString(SecureString password, ReadOnlySpanAction<char> action)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));
        var bStr = Marshal.SecureStringToBSTR(password);
        try
        {
            var bStrPtr = (char*)bStr;
            var bBtrLen = *(int*)(bStr - sizeof(int));
            var charSpan = new ReadOnlySpan<char>(bStrPtr, bBtrLen);
            action(charSpan);
        }
        finally
        {
            Marshal.ZeroFreeBSTR(bStr);
        }
    }
    private static unsafe TResult ReadSecureString<TResult>(SecureString password, ReadOnlySpanFunc<char, TResult> action)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));
        var bStr = Marshal.SecureStringToBSTR(password);
        try
        {
            var bStrPtr = (char*)bStr;
            var bBtrLen = *(int*)(bStr - sizeof(int));
            var charSpan = new ReadOnlySpan<char>(bStrPtr, bBtrLen);
            return action(charSpan);
        }
        finally
        {
            Marshal.ZeroFreeBSTR(bStr);
        }
    }

    private IEnumerable<byte[]> ExtractCerts()
    {
        Decrypt();

        return _info.AuthenticatedSafe
            .Where(safe => safe.GetBags().Any(bag => bag is Pkcs12CertBag))
            .SelectMany(safe => safe.GetBags().OfType<Pkcs12CertBag>())
            .Select(bag => ReadOctetString(bag.EncodedCertificate.Span, AsnEncodingRules.BER, out _));
    }

    public IEnumerable<X509Certificate> GetCertificatesFast()
        => ExtractCerts().Select(raw => new X509Certificate(raw));

    public IEnumerable<X509Certificate2> GetCertificatesSys()
        => ExtractCerts().Select(raw => new X509Certificate2(raw));

    public X509Certificate WithPrivateKey(X509Certificate cert, out Pkcs8PrivateKeyInfo pk)
    {
        if (cert is null) throw new ArgumentNullException(nameof(cert));

        var privateKeys = _info.AuthenticatedSafe
            .Where(safe => safe.GetBags().Any(bag => bag is Pkcs12ShroudedKeyBag))
            .SelectMany(safe => safe.GetBags().OfType<Pkcs12ShroudedKeyBag>())
            .Select(bag => _pw is not null
                ? ReadSecureString(_pw, chars => Pkcs8PrivateKeyInfo.DecryptAndDecode(chars, bag.EncryptedPkcs8PrivateKey, out _))
                : Pkcs8PrivateKeyInfo.DecryptAndDecode(ReadOnlySpan<byte>.Empty, bag.EncryptedPkcs8PrivateKey, out _));

        var algo = cert.CertificateStructure.SignatureAlgorithm.ToString();

        foreach (var info in privateKeys)
        {
            if (info.AlgorithmId.Value != algo) continue;

#pragma warning disable CA2000
            switch (algo)
            {
                // ECC
                case "1.2.840.10045.2.1": {
                    pk = info;
                    return cert;
                }
                // RSA
                case "1.2.840.113549.1.1.1": {
                    pk = info;
                    return cert;
                }
                // DSA
                case "1.2.840.10040.4.1": {
                    pk = info;
                    return cert;
                }
                default:
                    throw new NotImplementedException(new Oid(algo).FriendlyName);
            }
#pragma warning restore CA2000

        }
        throw new NotImplementedException("No compatible PKCS#8 private key found.");
    }

    public X509Certificate2 WithPrivateKey(X509Certificate2 cert)
    {
        if (cert is null) throw new ArgumentNullException(nameof(cert));

        var privateKeys = _info.AuthenticatedSafe
            .Where(safe => safe.GetBags().Any(bag => bag is Pkcs12ShroudedKeyBag))
            .SelectMany(safe => safe.GetBags().OfType<Pkcs12ShroudedKeyBag>())
            .Select(bag => ((Pkcs12ShroudedKeyBag Bag, Pkcs8PrivateKeyInfo Info))(_pw is not null
                ? (bag, ReadSecureString(_pw, chars => Pkcs8PrivateKeyInfo.DecryptAndDecode(chars, bag.EncryptedPkcs8PrivateKey, out _)))
                : (bag, Pkcs8PrivateKeyInfo.DecryptAndDecode(ReadOnlySpan<byte>.Empty, bag.EncryptedPkcs8PrivateKey, out _))));

        var algo = cert.GetKeyAlgorithm();
        foreach (var pk in privateKeys)
        {
            if (pk.Info.AlgorithmId.Value != algo) continue;

#pragma warning disable CA2000
            switch (algo)
            {
                // ECC
                case "1.2.840.10045.2.1": {
                    var ecdsa = ECDsa.Create();
                    ecdsa.ImportECPrivateKey(pk.Info.PrivateKeyBytes.Span, out _);
                    //ecdsa.ImportPkcs8PrivateKey(pk.Info.Encode(), out _);
                    //ecdsa.ImportPkcs8PrivateKey(pk.Bag.EncodedBagValue.Span, out _);
                    return cert.CopyWithPrivateKey(ecdsa);
                }
                // RSA
                case "1.2.840.113549.1.1.1": {
                    var rsa = RSA.Create();
                    rsa.ImportRSAPrivateKey(pk.Info.PrivateKeyBytes.Span, out _);
                    rsa.ImportPkcs8PrivateKey(pk.Bag.EncodedBagValue.Span, out _);
                    return cert.CopyWithPrivateKey(rsa);
                }
                // DSA
                case "1.2.840.10040.4.1": {
                    var dsa = DSA.Create();
                    //pk.dsa.ImportPkcs8PrivateKey(pk.PrivateKeyBytes.Span, out _);
                    dsa.ImportPkcs8PrivateKey(pk.Info.Encode(), out _);
                    return cert.CopyWithPrivateKey(dsa);
                }
                default:
                    throw new NotImplementedException(new Oid(algo).FriendlyName);
            }
#pragma warning restore CA2000

        }
        throw new NotImplementedException("No compatible PKCS#8 private key found.");
    }

    private void GetPkcs8Info() { }

    private void Decrypt(ReadOnlySpan<char> password)
    {
        foreach (var safe in _info.AuthenticatedSafe)
            if (safe.ConfidentialityMode == Pkcs12ConfidentialityMode.Password)
                safe.Decrypt(password);
    }

    private void Decrypt()
    {
        if (_pw is null)
            Decrypt(ReadOnlySpan<char>.Empty);
        else
            ReadSecureString(_pw, Decrypt);
    }

    public bool Equals(Pkcs12? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _rawData == other._rawData
            && _info == other._info;
    }

    public int CompareTo(Pkcs12? other)
    {
        if (other is null) return 1;

        return new BigSpan<byte>(_rawData).CompareMemory(other._rawData);
    }

    public override bool Equals(object? obj)
        => obj is Pkcs12 otherPkcs12 && Equals(otherPkcs12);

    public override int GetHashCode()
        => unchecked((int)Crc32C.Calculate(_rawData));

    // TODO:
    public override string ToString()
        => $"[Pkcs12Certificate 0x{GetHashCode():X8}]";
}
#endif
