using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using StirlingLabs.MsQuic.Bindings;
using NUnit.Framework;
using StirlingLabs.Utilities;
using StirlingLabs.Utilities.Assertions;
using static StirlingLabs.MsQuic.Bindings.MsQuic;

namespace StirlingLabs.MsQuic.Tests;

[Order(2)]
public class Quic0RttTests
{
    private QuicRegistration _reg = null!;

    private QuicClientConfiguration _clientCfg = null!;
    private QuicClientConnection _clientSide = null!;

    private QuicListener _listener = null!;
    private QuicServerConfiguration _listenerCfg = null!;
    private QuicServerConnection? _serverSide;

    private static ushort _lastPort = 33999;
    private static QuicCertificate _cert;
    private ushort _port;
    private Memory<byte> _ticket;

    private static readonly bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (IsContinuousIntegration)
            Trace.Listeners.Add(new ConsoleTraceListener());

        var asmDir = Path.GetDirectoryName(new Uri(typeof(RoundTripTests).Assembly.Location).LocalPath);
        var p12Path = Path.Combine(asmDir!, "localhost.p12");

        _cert = new(File.OpenRead(p12Path));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
        => _cert.Free();

    [SetUp]
    public void SetUp()
    {
        var output = TestContext.Out;

        output.WriteLine($"=== SETUP {TestContext.CurrentContext.Test.FullName} ===");

        _port = _lastPort += 1;

        var testName = TestContext.CurrentContext.Test.FullName;

        _reg = new(testName);

        _listenerCfg = new(_reg, "test");

        _listenerCfg.ConfigureCredentials(_cert);

        _listener = new(_listenerCfg);

        _listener.Start(new(IPAddress.IPv6Loopback, _port));

        _listener.UnobservedException += (_, info) => {
            info.Throw();
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
            _serverSide = connection;
            _serverSide.UnobservedException += (_, info) => {
                info.Throw();
            };
        };

        _clientCfg = new(_reg, "test");

        _clientCfg.ConfigureCredentials();

        // get resumption ticket
        {

            _clientSide = new(_clientCfg);

            _clientSide.ResumptionTicketReceived += c => {
                _ticket = c.ResumptionTicket;
            };

            // connection
            _clientSide.CertificateReceived += (peer, certificate, chain, flags, status)
                => {
                // TODO: cheap cert validation tests
                return QUIC_STATUS_SUCCESS;
            };

            _clientSide.UnobservedException += (_, info) => {
                info.Throw();
            };

            using var cde = new CountdownEvent(2);

            _clientSide.Connected += _ => {
                cde.Signal();
            };
            _clientSide.ResumptionTicketReceived += _ => {
                cde.Signal();
            };

            _clientSide.Start("localhost", _port);

            cde.Wait();

            _clientSide.Close();

            _clientSide.Dispose();
        }

        // setup connection to resume
        {

            _clientSide = new(_clientCfg);

            _clientSide.SetResumptionTicket(_ticket);

            // connection
            _clientSide.CertificateReceived += (peer, certificate, chain, flags, status)
                => {
                // TODO: cheap cert validation tests
                return QUIC_STATUS_SUCCESS;
            };

            _clientSide.UnobservedException += (_, info) => {
                info.Throw();
            };
        }

        output.WriteLine($"=== BEGIN {TestContext.CurrentContext.Test.FullName} ===");
    }

    [TearDown]
    public void TearDown()
    {
        _serverSide?.Dispose();
        _clientSide.Dispose();
        _clientCfg.Dispose();
        _listener.Dispose();
        _listenerCfg.Dispose();
        _reg.Dispose();

        TestContext.Out.WriteLine($"=== END {TestContext.CurrentContext.Test.FullName} ===");
    }


    [Test]
    [Timeout(20000)]
    public unsafe void RoundTrip0RttStreamTest()
    {
        // stream round trip
        Memory<byte> utf8Hello = Encoding.UTF8.GetBytes("Hello");
        var dataLength = utf8Hello.Length;

        using var cde = new CountdownEvent(2);

        using var clientStream = _clientSide.OpenStream(true);

        Debug.Assert(!clientStream.IsStarted);

        var streamOpened = false;
        var was0Rtt = false;

        QuicStream serverStream = null !;

        int read = 0;
        Span<byte> dataReceived = stackalloc byte[dataLength];

        fixed (byte* pDataReceived = dataReceived)
        {
            var ptrDataReceived = (IntPtr)pDataReceived;

            _listener.ClientConnected += (_, connection) => {
                _serverSide = connection;
                _serverSide.UnobservedException += (_, info) => {
                    info.Throw();
                };
                _serverSide.IncomingStream += (_, stream) => {
                    serverStream = stream;
                    serverStream.DataReceived += x => {
                        was0Rtt = (x.LastReceiveFlags & QUIC_RECEIVE_FLAGS.ZERO_RTT) != 0;

                        // ReSharper disable once VariableHidesOuterVariable
                        var dataReceived = new Span<byte>((byte*)ptrDataReceived, dataLength);
                        read = serverStream.Receive(dataReceived);
                        cde.Signal();
                    };
                    streamOpened = true;
                    cde.Signal();
                };
            };
        }

        var task = clientStream.SendAsync(utf8Hello,
            QUIC_SEND_FLAGS.ALLOW_0_RTT
            | QUIC_SEND_FLAGS.FIN);

        clientStream.Start();
        _clientSide.Start("localhost", _port);

        cde.Wait();

        Assert.True(streamOpened);

        Assert.True(was0Rtt);

        Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {serverStream} Completed Receive");

        Assert.AreEqual(dataLength, read);

        BigSpanAssert.AreEqual<byte>(utf8Hello.Span, dataReceived);

        task.Wait();

        Assert.True(task.IsCompletedSuccessfully);

    }
}
