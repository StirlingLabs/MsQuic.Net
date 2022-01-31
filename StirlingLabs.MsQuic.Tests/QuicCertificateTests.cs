using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace StirlingLabs.MsQuic.Tests;

[Order(0)]
public class QuicCertificateTests
{
    private static ushort _lastPort = 32999;
    
    [Test]
    [Platform("Win")]
    public void StartUpCertFromStoreTest()
    {
        var testName = TestContext.CurrentContext.Test.FullName;

        using var reg = new QuicRegistration(testName);

        using var listenerCfg = new QuicServerConfiguration(reg, "test");

        var cert = new QuicCertificate("958478283cc9615d1bfbd90f19aed982e6ae1034");

        listenerCfg.ConfigureCredentials(cert);

        cert.Free();

        using var listener = new QuicListener(listenerCfg);

        listener.Start(new(IPAddress.IPv6Loopback, _lastPort += 1));

    }

    [Test]
    public void StartUpCertFromP12Test()
    {
        var testName = TestContext.CurrentContext.Test.FullName;

        using var reg = new QuicRegistration(testName);

        using var listenerCfg = new QuicServerConfiguration(reg, "test");

        var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath)!;
        var p12Path = Path.Combine(asmDir, "localhost.p12");

        var cert = new QuicCertificate(policy => {
            policy.RevocationMode = X509RevocationMode.NoCheck;
            policy.DisableCertificateDownloads = false;
            policy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority
                | X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown
                | X509VerificationFlags.IgnoreCtlSignerRevocationUnknown
                | X509VerificationFlags.IgnoreRootRevocationUnknown
                | X509VerificationFlags.IgnoreEndRevocationUnknown;
        }, File.OpenRead(p12Path));

        listenerCfg.ConfigureCredentials(cert);

        cert.Free();

        using var listener = new QuicListener(listenerCfg);

        listener.Start(new(IPAddress.IPv6Loopback, _lastPort += 1));
    }
}
