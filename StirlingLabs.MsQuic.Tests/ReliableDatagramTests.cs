using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StirlingLabs.MsQuic.Bindings;
using NUnit.Framework;
using StirlingLabs.Utilities;
using StirlingLabs.Utilities.Assertions;
using static StirlingLabs.MsQuic.Bindings.MsQuic;

namespace StirlingLabs.MsQuic.Tests;

[Order(1)]
public class ReliableDatagramTests
{
    private QuicServerConnection _serverSide = null!;
    private QuicClientConnection _clientSide = null!;
    private QuicListener _listener = null!;
    private static ushort _lastPort = 34999;
    private static QuicCertificate _cert;
    private QuicRegistration _reg = null!;

    private static readonly bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (IsContinuousIntegration)
            Trace.Listeners.Add(new ConsoleTraceListener());

        var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath);
        var p12Path = Path.Combine(asmDir!, "localhost.p12");

        _cert = new(policy => {
            policy.RevocationMode = X509RevocationMode.NoCheck;
            policy.DisableCertificateDownloads = false;
            policy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority
                | X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown
                | X509VerificationFlags.IgnoreCtlSignerRevocationUnknown
                | X509VerificationFlags.IgnoreRootRevocationUnknown
                | X509VerificationFlags.IgnoreEndRevocationUnknown;
        }, File.OpenRead(p12Path));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
        => _cert.Free();

    [SetUp]
    public void SetUp()
    {
        var output = TestContext.Out;

        output.WriteLine($"=== SETUP {TestContext.CurrentContext.Test.FullName} ===");

        var port = _lastPort += 1;

        var testName = TestContext.CurrentContext.Test.FullName;

        _reg = new(testName);

        using var listenerCfg = new QuicServerConfiguration(_reg, true, "test");

        listenerCfg.ConfigureCredentials(_cert);

        _listener = new(listenerCfg);

        _listener.Start(new(IPAddress.IPv6Loopback, port));

        using var clientCfg = new QuicClientConfiguration(_reg, true, "test");

        clientCfg.ConfigureCredentials();

        _clientSide = new(clientCfg) { Name = "Client" };

        // connection
        using var cde = new CountdownEvent(2);
        var clientConnected = false;
        var serverConnected = false;

        _clientSide.CertificateReceived += (peer, certificate, chain, flags, status)
            => {
            // TODO: cheap cert validation tests
            return QUIC_STATUS_SUCCESS;
        };

        _listener.NewConnection += (_, connection) => {
            output.WriteLine("handling _listener.NewConnection");
            _serverSide = connection;
            connection.CertificateReceived += (_, _, _, _, _)
                => {
                output.WriteLine("handled server CertificateReceived");
                // TODO: cheap cert validation tests
                return QUIC_STATUS_SUCCESS;
            };
            output.WriteLine("handled _listener.NewConnection");
        };

        _listener.ClientConnected += (_, connection) => {
            serverConnected = true;
            _serverSide = connection;
            _serverSide.Name = "Server";
            cde.Signal();
        };

        _clientSide.Connected += _ => {
            clientConnected = true;
            cde.Signal();
        };

        _clientSide.Start("localhost", port);

        cde.Wait();

        Assert.True(serverConnected);
        Assert.True(clientConnected);

        _listener.UnobservedException += (_, info) => {
            info.Throw();
        };

        _clientSide.UnobservedException += (_, info) => {
            info.Throw();
        };

        Assert.NotNull(_serverSide);

        _serverSide.UnobservedException += (_, info) => {
            info.Throw();
        };

        output.WriteLine($"=== BEGIN {TestContext.CurrentContext.Test.FullName} ===");
    }

    [TearDown]
    public void TearDown()
    {
        _serverSide.Dispose();
        _clientSide.Dispose();
        _listener.Dispose();
        _reg.Dispose();

        TestContext.Out.WriteLine($"=== END {TestContext.CurrentContext.Test.FullName} ===");
    }


    [Order(0)]
    [Test]
    [Timeout(1000)]
    public void RoundTripSanityTest()
    {
        // intentionally empty
    }

    [Test]
    [Timeout(10000)]
    public void RoundTripDatagramTest()
    {
        // datagram round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");

        using var datagram = (IQuicDatagramReliable)QuicDatagram.Create(_clientSide, utf8Hello);

        using var cde = new CountdownEvent(4);
        var dgSent = false;
        var dgAcknowledged = false;
        var dgReceived = false;
        var dgReliablyAcknowledged = false;

        datagram.StateChanged += (_, state) => {
            switch (state)
            {
                case QUIC_DATAGRAM_SEND_STATE.SENT:
                    dgSent = true;
                    cde.Signal();
                    break;
                case QUIC_DATAGRAM_SEND_STATE.ACKNOWLEDGED:
                case QUIC_DATAGRAM_SEND_STATE.ACKNOWLEDGED_SPURIOUS when Debugger.IsAttached:
                    dgAcknowledged = true;
                    cde.Signal();
                    break;
                case QUIC_DATAGRAM_SEND_STATE.LOST_SUSPECT when Debugger.IsAttached:
                    // ok
                    break;
                case QUIC_DATAGRAM_SEND_STATE.LOST_DISCARDED:
                case QUIC_DATAGRAM_SEND_STATE.CANCELED:
                default:
                    Assert.Fail(state.ToString());
                    return;
            }
        };

        _serverSide.DatagramReceived += (_, bytes) => {
            dgReceived = true;
            BigSpanAssert.AreEqual<byte>(utf8Hello.Span, bytes);
            cde.Signal();
        };

        datagram.ReliablyAcknowledged += _ => {
            dgReliablyAcknowledged = true;
            cde.Signal();
        };

        _clientSide.SendDatagram(datagram);

        var success = cde.Wait(8000);
        Assert.Multiple(() => {
            Assert.True(dgSent, "Datagram must be sent");
            Assert.True(dgAcknowledged, "Datagram must be acknowledged");
            Assert.True(dgReceived, "Datagram must be received");
            Assert.True(dgReliablyAcknowledged, "Datagram must be reliably acknowledged");
        });
        Assert.True(success, "Wait must complete before timeout.");
    }


    [Test]
    [Timeout(10000)]
    public void RoundTripDatagram2Test()
    {
        // datagram round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");

        using var dg = QuicDatagram.Create(_clientSide, utf8Hello);

        using var cde = new CountdownEvent(3);
        var dgSent = false;
        var dgAcknowledged = false;
        var dgReceived = false;

        TestContext.Out.WriteLine("configured async wait for datagram sent");
        dg.WaitForSentAsync()
            .ContinueWith((t, o) => {
                TestContext.Out.WriteLine("datagram sent");
                if (!t.IsCompletedSuccessfully)
                {
                    TestContext.Out.WriteLine($"datagram sent unsuccessful, status: {t.Status}");
                    if (t.Exception is not null)
                        TestContext.Out.WriteLine(t.Exception.ToString());
                    Assert.Fail(t.Status.ToString());
                    return;
                }
                dgSent = true;
                ((CountdownEvent)o!).Signal();
                TestContext.Out.WriteLine("handled datagram sent");
            }, cde, TaskScheduler.Default);

        TestContext.Out.WriteLine("configured async wait for datagram acknowledgement");
        dg.WaitForAcknowledgementAsync()
            .ContinueWith((t, o) => {
                TestContext.Out.WriteLine("datagram acknowledgement arrived");
                if (!t.IsCompletedSuccessfully)
                {
                    TestContext.Out.WriteLine($"datagram acknowledgement unsuccessful, status: {t.Status}");
                    if (t.Exception is not null)
                        TestContext.Out.WriteLine(t.Exception.ToString());
                    Assert.Fail(t.Status.ToString());
                    return;
                }
                dgAcknowledged = true;
                ((CountdownEvent)o!).Signal();
                TestContext.Out.WriteLine("handled datagram acknowledgement");
            }, cde, TaskScheduler.Default);

        _serverSide.DatagramReceived += (_, bytes) => {
            TestContext.Out.WriteLine("DatagramReceived event");
            dgReceived = true;
            BigSpanAssert.AreEqual<byte>(utf8Hello.Span, bytes);
            cde.Signal();
            TestContext.Out.WriteLine("handled server DatagramReceived");
        };

        TestContext.Out.WriteLine("sending datagram");
        _clientSide.SendDatagram(dg);

        TestContext.Out.WriteLine("waiting on async events");
        cde.Wait();

        TestContext.Out.WriteLine("waiting finished");
        Assert.True(dgSent);
        Assert.True(dgAcknowledged);
        Assert.True(dgReceived);
    }
}
