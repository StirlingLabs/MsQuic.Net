using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Quic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using StirlingLabs.Utilities.Assertions;
using static Microsoft.Quic.MsQuic;

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

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {

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
    }

    [TearDown]
    public void TearDown()
    {
        _serverSide.Dispose();
        _clientSide.Dispose();
        _listener.Dispose();
        _reg.Dispose();
    }

    [Order(0)]
    [Test]
    public void RoundTripSanityTest()
    {
        // intentionally empty
    }

    [Test]
    public void RoundTripDatagramTest()
    {
        // datagram round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");

        using var datagram = (QuicDatagramReliable)QuicDatagram.Create(_clientSide, utf8Hello);

        using var cde = new CountdownEvent(4);
        var dgSent = false;
        var dgAcknowledged = false;
        var dgReceived = false;
        var dgReliablyAcknowledged = false;

        datagram.StateChanged += (_, state) => {
            switch (state)
            {
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_SENT:
                    dgSent = true;
                    cde.Signal();
                    break;
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_ACKNOWLEDGED:
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_ACKNOWLEDGED_SPURIOUS when Debugger.IsAttached:
                    dgAcknowledged = true;
                    cde.Signal();
                    break;
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_LOST_SUSPECT when Debugger.IsAttached:
                    // ok
                    break;
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_LOST_DISCARDED:
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_CANCELED:
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

        cde.Wait();

        Assert.True(dgSent);
        Assert.True(dgAcknowledged);
        Assert.True(dgReceived);
        Assert.True(dgReliablyAcknowledged);
    }


    [Test]
    public void RoundTripDatagram2Test()
    {
        // datagram round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");

        using var dg = QuicDatagram.Create(_clientSide, utf8Hello);

        using var cde = new CountdownEvent(3);
        var dgSent = false;
        var dgAcknowledged = false;
        var dgReceived = false;

        dg.WaitForSentAsync()
            .ContinueWith((t, o) => {
                if (!t.IsCompletedSuccessfully)
                    Assert.Fail(t.Status.ToString());
                dgSent = true;
                ((CountdownEvent)o!).Signal();
            }, cde, TaskScheduler.Default);

        dg.WaitForAcknowledgementAsync()
            .ContinueWith((t, o) => {
                if (!t.IsCompletedSuccessfully)
                    Assert.Fail(t.Status.ToString());
                dgAcknowledged = true;
                ((CountdownEvent)o!).Signal();
            }, cde, TaskScheduler.Default);

        _serverSide.DatagramReceived += (_, bytes) => {
            dgReceived = true;
            BigSpanAssert.AreEqual<byte>(utf8Hello.Span, bytes);
            cde.Signal();
        };

        _clientSide.SendDatagram(dg);

        cde.Wait();

        Assert.True(dgSent);
        Assert.True(dgAcknowledged);
        Assert.True(dgReceived);
    }
}
