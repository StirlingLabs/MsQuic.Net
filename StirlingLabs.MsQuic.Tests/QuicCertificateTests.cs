using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using StirlingLabs.Utilities;

namespace StirlingLabs.MsQuic.Tests;

[Order(0)]
public class QuicCertificateTests
{
    private static ushort _lastPort = 32999;

    private static readonly bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (IsContinuousIntegration)
            Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [SetUp]
    public void SetUp()
        => TestContext.Progress.WriteLine($"=== BEGIN {TestContext.CurrentContext.Test.FullName} ===");

    [TearDown]
    public void TearDown()
        => TestContext.Progress.WriteLine($"=== END {TestContext.CurrentContext.Test.FullName} ===");

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

        TestContext.Progress.WriteLine("Creating QuicServerConfiguration");

        using var listenerCfg = new QuicServerConfiguration(reg, "test");

        var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath)!;
        var p12Path = Path.Combine(asmDir, "localhost.p12");

        TestContext.Progress.WriteLine("Creating QuicCertificate");

        var cert = new QuicCertificate(policy => {
            policy.RevocationMode = X509RevocationMode.NoCheck;
            policy.DisableCertificateDownloads = false;
            policy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority
                | X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown
                | X509VerificationFlags.IgnoreCtlSignerRevocationUnknown
                | X509VerificationFlags.IgnoreRootRevocationUnknown
                | X509VerificationFlags.IgnoreEndRevocationUnknown;
        }, File.OpenRead(p12Path));

        TestContext.Progress.WriteLine("QuicServerConfiguration.ConfigureCredentials");

        listenerCfg.ConfigureCredentials(cert);

        TestContext.Progress.WriteLine("QuicCertificate.Free");

        cert.Free();

        TestContext.Progress.WriteLine("Creating QuicListener");

        using var listener = new QuicListener(listenerCfg);

        TestContext.Progress.WriteLine("QuicListener.Start");

        listener.Start(new(IPAddress.IPv6Loopback, _lastPort += 1));

        TestContext.Progress.WriteLine("Disposal");
    }
}
