using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Quic;
using NUnit.Framework;
using StirlingLabs.Utilities;
using StirlingLabs.Utilities.Assertions;
using static Microsoft.Quic.MsQuic;

namespace StirlingLabs.MsQuic.Tests;

[Order(1)]
public class RoundTripTests
{
    private QuicServerConnection _serverSide = null!;
    private QuicClientConnection _clientSide = null!;
    private QuicListener _listener = null!;
    private static ushort _lastPort = 31999;
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
        TestContext.Progress.WriteLine($"=== SETUP {TestContext.CurrentContext.Test.FullName} ===");

        var port = _lastPort += 1;

        var testName = TestContext.CurrentContext.Test.FullName;

        _reg = new(testName);

        using var listenerCfg = new QuicServerConfiguration(_reg, "test");

        listenerCfg.ConfigureCredentials(_cert);

        _listener = new(listenerCfg);

        _listener.Start(new(IPAddress.IPv6Loopback, port));

        using var clientCfg = new QuicClientConfiguration(_reg, "test");

        clientCfg.ConfigureCredentials();

        _clientSide = new(clientCfg);

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

        TestContext.Progress.WriteLine($"=== BEGIN {TestContext.CurrentContext.Test.FullName} ===");
    }

    [TearDown]
    public void TearDown()
    {
        _serverSide.Dispose();
        _clientSide.Dispose();
        _listener.Dispose();
        _reg.Dispose();

        TestContext.Progress.WriteLine($"=== END {TestContext.CurrentContext.Test.FullName} ===");
    }

    [Order(0)]
    [Test]
    [Timeout(100)]
    public void RoundTripSanityTest()
    {
        // intentionally empty
    }

    [Test]
    [Timeout(10000)]
    public unsafe void RoundTripSimpleStreamTest()
    {
        // stream round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");
        var dataLength = utf8Hello.Length;

        using var cde = new CountdownEvent(1);

        using var clientStream = _clientSide.OpenStream();

        var streamOpened = false;

        //clientStream.Send(utf8Hello);

        QuicStream serverStream = null !;

        _serverSide.IncomingStream += (_, stream) => {
            TestContext.Progress.WriteLine("handling _serverSide.IncomingStream");
            serverStream = stream;
            streamOpened = true;
            cde.Signal();
            TestContext.Progress.WriteLine("handled _serverSide.IncomingStream");
        };

        TestContext.Progress.WriteLine("waiting for _serverSide.IncomingStream");
        cde.Wait();

        Assert.True(streamOpened);

        cde.Reset();

        Span<byte> dataReceived = stackalloc byte[dataLength];

        fixed (byte* pDataReceived = dataReceived)
        {
            var ptrDataReceived = (IntPtr)pDataReceived;

            serverStream.DataReceived += _ => {
                TestContext.Progress.WriteLine("handling serverStream.DataReceived");

                // ReSharper disable once VariableHidesOuterVariable
                var dataReceived = new Span<byte>((byte*)ptrDataReceived, dataLength);

                var read = serverStream.Receive(dataReceived);

                Assert.AreEqual(dataLength, read);

                cde.Signal();
                TestContext.Progress.WriteLine("handled serverStream.DataReceived");
            };

        }

        var task = clientStream.SendAsync(utf8Hello, QUIC_SEND_FLAGS.QUIC_SEND_FLAG_FIN);

        TestContext.Progress.WriteLine("waiting for serverStream.DataReceived");
        cde.Wait();

        BigSpanAssert.AreEqual<byte>(utf8Hello.Span, dataReceived);

        TestContext.Progress.WriteLine("waiting for clientStream.SendAsync");
        task.Wait();

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Test]
    [Timeout(10000)]
    [Ignore("Not currently supported, may be deprecated")]
    public unsafe void RoundTripSimple2StreamTest()
    {
        // stream round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");
        var dataLength = utf8Hello.Length;

        using var cde = new CountdownEvent(1);

        using var clientStream = _clientSide.OpenStream();

        var streamOpened = false;

        //clientStream.Send(utf8Hello);

        QuicStream serverStream = null !;

        _serverSide.IncomingStream += (_, stream) => {
            serverStream = stream;
            streamOpened = true;
            cde.Signal();
        };

        cde.Wait();

        Assert.True(streamOpened);

        cde.Reset();

        serverStream.DataReceived += _ => {
            cde.Signal();
        };

        var task = clientStream.SendAsync(utf8Hello, QUIC_SEND_FLAGS.QUIC_SEND_FLAG_FIN);

        cde.Wait();

        Span<byte> dataReceived = stackalloc byte[dataLength];

        var read = serverStream.Receive(dataReceived);

        Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {serverStream} Completed Receive");

        Assert.AreEqual(dataLength, read);

        BigSpanAssert.AreEqual<byte>(utf8Hello.Span, dataReceived);

        task.Wait();

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Test]
    [Timeout(10000)]
    public void RoundTripDatagramTest()
    {
        // datagram round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");

        using var datagram = new QuicDatagramManagedMemory(_clientSide, utf8Hello);

        using var cde = new CountdownEvent(3);
        var dgSent = false;
        var dgAcknowledged = false;
        var dgReceived = false;

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

        _clientSide.SendDatagram(datagram);

        cde.Wait();

        Assert.True(dgSent);
        Assert.True(dgAcknowledged);
        Assert.True(dgReceived);
    }


    [Test]
    [Timeout(10000)]
    public void RoundTripDatagram2Test()
    {
        // datagram round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");

        using var dg = new QuicDatagramManagedMemory(_clientSide, utf8Hello);

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
