using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X509;
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
        => TestContext.Out.WriteLine($"=== BEGIN {TestContext.CurrentContext.Test.FullName} ===");

    [TearDown]
    public void TearDown()
        => TestContext.Out.WriteLine($"=== END {TestContext.CurrentContext.Test.FullName} ===");
    
    [Test]
    [Platform("Win")]
    [Explicit]
    public void StartUpCertFromStoreTest()
    {
        if (IsContinuousIntegration)
            Assert.Ignore("Certificate store is not expected to be set up under CI.");

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
    [Repeat(10)]
    public void StartUpCertFromP12Test()
    {
        var output = TestContext.Out;

        var testName = TestContext.CurrentContext.Test.FullName;

        using var reg = new QuicRegistration(testName);

        output.WriteLine("Creating QuicServerConfiguration");

        using var listenerCfg = new QuicServerConfiguration(reg, "test");

        var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath)!;
        var p12Path = Path.Combine(asmDir, "localhost.p12");

        output.WriteLine("Creating QuicCertificate");
        var sw = Stopwatch.StartNew();
        var cert = new QuicCertificate(File.OpenRead(p12Path));
        var elapsed = sw.ElapsedTicks;
        TestContext.WriteLine($"QuicCertificate ctor {elapsed} ({elapsed / (Stopwatch.Frequency / 1000.0):F1}ms)");

        output.WriteLine("QuicServerConfiguration.ConfigureCredentials");

        listenerCfg.ConfigureCredentials(cert);

        output.WriteLine("QuicCertificate.Free");

        cert.Free();

        output.WriteLine("Creating QuicListener");

        using var listener = new QuicListener(listenerCfg);

        output.WriteLine("QuicListener.Start");

        listener.Start(new(IPAddress.IPv6Loopback, _lastPort += 1));

        output.WriteLine("Disposal");
    }
}
