using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using StirlingLabs.MsQuic.Bindings;
using NUnit.Framework;
using StirlingLabs.Utilities;
using StirlingLabs.Utilities.Assertions;
using static StirlingLabs.MsQuic.Bindings.MsQuic;

namespace StirlingLabs.MsQuic.Tests;

[Order(1)]
public class CertlessRoundTripTests
{
    private QuicServerConnection _serverSide = null!;
    private QuicClientConnection _clientSide = null!;
    private QuicListener _listener = null!;
    private static ushort _lastPort = 31999;
    private static QuicCertificate _cert;
    private QuicRegistration _reg = null!;

    private static readonly bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");
    private TextWriterTraceListener _tl;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (IsContinuousIntegration)
            Trace.Listeners.Add(new ConsoleTraceListener(true));

        var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath);
        var p12Path = Path.Combine(asmDir!, "localhost.p12");

        _cert = new( File.OpenRead(p12Path));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
        => _cert.Free();

    [SetUp]
    public void SetUp()
    {
        var output = TestContext.Out;

        _tl = new(output);

        Trace.Listeners.Add(_tl);

        output.WriteLine($"=== SETUP {TestContext.CurrentContext.Test.FullName} ===");

        var port = _lastPort += 1;

        var testName = TestContext.CurrentContext.Test.FullName;

        _reg = new(testName);

        using var listenerCfg = new QuicServerConfiguration(_reg, "test");

        listenerCfg.ConfigureCredentials(_cert, QUIC_CREDENTIAL_FLAGS.NO_CERTIFICATE_VALIDATION);

        _listener = new(listenerCfg);

        output.WriteLine("starting _listener");

        _listener.Start(new(IPAddress.IPv6Loopback, port));

        using var clientCfg = new QuicClientConfiguration(_reg, "test");

        clientCfg.ConfigureCredentials(QUIC_CREDENTIAL_FLAGS.NO_CERTIFICATE_VALIDATION);

        _clientSide = new(clientCfg);

        // connection
        using var cde = new CountdownEvent(2);
        var clientConnected = false;
        var serverConnected = false;

        _clientSide.CertificateReceived += (_, _, _, _, _)
            => {
            output.WriteLine("handled client CertificateReceived");
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
            output.WriteLine("handling _listener.ClientConnected");
            serverConnected = true;
            _serverSide.Should().Be(connection);
            cde.Signal();
            output.WriteLine("handled _listener.ClientConnected");
        };

        _clientSide.Connected += _ => {
            output.WriteLine("handling _clientSide.Connected");
            clientConnected = true;
            cde.Signal();
            output.WriteLine("handled _clientSide.Connected");
        };

        output.WriteLine("starting _clientSide");

        _clientSide.Start("localhost", port);

        output.WriteLine("waiting for _listener.ClientConnected, _clientSide.Connected");

        cde.Wait();

        Assert.True(serverConnected);
        Assert.True(clientConnected);

        _listener.UnobservedException += (_, info) => {
            Assert.Warn("_listener.UnobservedException");
            info.Throw();
        };

        _clientSide.UnobservedException += (_, info) => {
            Assert.Warn("_clientSide.UnobservedException");
            info.Throw();
        };

        Assert.NotNull(_serverSide);

        _serverSide.UnobservedException += (_, info) => {
            Assert.Warn("_serverSide.UnobservedException");
            info.Throw();
        };

        output.WriteLine($"=== BEGIN {TestContext.CurrentContext.Test.FullName} ===");
    }

    [TearDown]
    public void TearDown()
    {
        Trace.Listeners.Remove(_tl);

        _serverSide.Dispose();
        _clientSide.Dispose();
        _listener.Dispose();
        _reg.Dispose();

        _tl = null!;

        TestContext.Out.WriteLine($"=== END {TestContext.CurrentContext.Test.FullName} ===");
    }

    [Order(0)]
    [Test]
    [Timeout(20000)]
    public void RoundTripSanityTest()
    {
        // intentionally empty
    }
    
    [Test]
    [Timeout(20000)]
    public unsafe void RoundTripClientShutdownTest()
    {
        var output = TestContext.Out;

        using var cde = new CountdownEvent(1);

        var streamOpened = false;

        //clientStream.Send(utf8Hello);

        QuicStream serverStream = null !;

        _serverSide.IncomingStream += (_, stream) => {
            serverStream = stream;
            streamOpened = true;
            cde.Signal();
            output.WriteLine("handled _serverSide.IncomingStream");
        };

        output.WriteLine("waiting for _serverSide.IncomingStream");

        using var clientStream = _clientSide.OpenStream();

        cde.Wait();

        Assert.True(streamOpened);

        cde.Reset(2);

        _clientSide.ConnectionShutdown += (_, _, initByTransport, initByPeer) => {
            Assert.False(initByTransport);
            Assert.False(initByPeer);
            cde.Signal();
            output.WriteLine("handled _clientSide.ConnectionShutdown");
        };

        _serverSide.ConnectionShutdown += (_, _, initByTransport, initByPeer) => {
            Assert.False(initByTransport);
            Assert.True(initByPeer);
            cde.Signal();
            output.WriteLine("handled _serverSide.ConnectionShutdown");
        };
        
        _clientSide.Shutdown();
        cde.Wait();
        
        
    }

    [Test]
    [Timeout(20000)]
    public unsafe void RoundTripServerShutdownTest()
    {
        var output = TestContext.Out;

        using var cde = new CountdownEvent(1);

        var streamOpened = false;

        //clientStream.Send(utf8Hello);

        QuicStream serverStream = null !;

        _serverSide.IncomingStream += (_, stream) => {
            serverStream = stream;
            streamOpened = true;
            cde.Signal();
            output.WriteLine("handled _serverSide.IncomingStream");
        };

        output.WriteLine("waiting for _serverSide.IncomingStream");

        using var clientStream = _clientSide.OpenStream();

        cde.Wait();

        Assert.True(streamOpened);

        cde.Reset(2);

        _clientSide.ConnectionShutdown += (_, _, initByTransport, initByPeer) => {
            Assert.False(initByTransport);
            Assert.True(initByPeer);
            cde.Signal();
            output.WriteLine("handled _clientSide.ConnectionShutdown");
        };

        _serverSide.ConnectionShutdown += (_, _, initByTransport, initByPeer) => {
            Assert.False(initByTransport);
            Assert.False(initByPeer);
            cde.Signal();
            output.WriteLine("handled _serverSide.ConnectionShutdown");
        };
        
        _serverSide.Shutdown();
        cde.Wait();
        
        
    }
    [Test]
    [Timeout(20000)]
    public unsafe void RoundTripSimpleStreamTest()
    {
        var output = TestContext.Out;

        // stream round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");
        var dataLength = utf8Hello.Length;

        using var cde = new CountdownEvent(1);

        var streamOpened = false;

        //clientStream.Send(utf8Hello);

        QuicStream serverStream = null !;

        _serverSide.IncomingStream += (_, stream) => {
            output.WriteLine("handling _serverSide.IncomingStream");
            serverStream = stream;
            streamOpened = true;
            cde.Signal();
            output.WriteLine("handled _serverSide.IncomingStream");
        };

        output.WriteLine("waiting for _serverSide.IncomingStream");

        using var clientStream = _clientSide.OpenStream();

        cde.Wait();

        Assert.True(streamOpened);

        cde.Reset();

        Span<byte> dataReceived = stackalloc byte[dataLength];

        fixed (byte* pDataReceived = dataReceived)
        {
            var ptrDataReceived = (IntPtr)pDataReceived;

            serverStream.DataReceived += _ => {
                output.WriteLine("handling serverStream.DataReceived");

                // ReSharper disable once VariableHidesOuterVariable
                var dataReceived = new Span<byte>((byte*)ptrDataReceived, dataLength);

                var read = serverStream.Receive(dataReceived);

                Assert.IsTrue(dataLength.Equals(read));

                cde.Signal();
                output.WriteLine("handled serverStream.DataReceived");
            };

        }

        var task = clientStream.SendAsync(utf8Hello, QUIC_SEND_FLAGS.FIN);

        output.WriteLine("waiting for serverStream.DataReceived");
        cde.Wait();

        BigSpanAssert.AreEqual<byte>(utf8Hello.Span, dataReceived);

        output.WriteLine("waiting for clientStream.SendAsync");
        task.Wait();

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Test]
    [Timeout(20000)]
    public unsafe void RoundTripSimpleQueuedStreamTest()
    {
        var output = TestContext.Out;

        // stream round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");
        var dataLength = utf8Hello.Length;

        using var cde = new CountdownEvent(1);

        var streamOpened = false;

        //clientStream.Send(utf8Hello);

        QuicStream serverStream = null !;

        output.WriteLine("calling _clientSide.OpenStream");

        using var clientStream = _clientSide.OpenStream();

        while (_serverSide.QueuedIncomingStreams < 1)
            Thread.Yield();

        _serverSide.IncomingStream += (_, stream) => {
            output.WriteLine("handling _serverSide.IncomingStream");
            serverStream = stream;
            streamOpened = true;
            cde.Signal();
            output.WriteLine("handled _serverSide.IncomingStream");
        };

        output.WriteLine("waiting for _serverSide.IncomingStream");

        cde.Wait();

        Assert.True(streamOpened);

        cde.Reset();

        Span<byte> dataReceived = stackalloc byte[dataLength];

        fixed (byte* pDataReceived = dataReceived)
        {
            var ptrDataReceived = (IntPtr)pDataReceived;

            serverStream.DataReceived += _ => {
                output.WriteLine("handling serverStream.DataReceived");

                // ReSharper disable once VariableHidesOuterVariable
                var dataReceived = new Span<byte>((byte*)ptrDataReceived, dataLength);

                var read = serverStream.Receive(dataReceived);

                Assert.AreEqual(dataLength, read);

                cde.Signal();
                output.WriteLine("handled serverStream.DataReceived");
            };

        }

        var task = clientStream.SendAsync(utf8Hello, QUIC_SEND_FLAGS.FIN);

        output.WriteLine("waiting for serverStream.DataReceived");
        cde.Wait();

        BigSpanAssert.AreEqual<byte>(utf8Hello.Span, dataReceived);

        output.WriteLine("waiting for clientStream.SendAsync");
        task.Wait();

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Test]
    [Timeout(20000)]
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

        _clientSide.SendDatagram(datagram);

        cde.Wait();

        Assert.True(dgSent);
        Assert.True(dgAcknowledged);
        Assert.True(dgReceived);
    }


    [Test]
    [Timeout(20000)]
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
