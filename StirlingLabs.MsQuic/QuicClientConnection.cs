using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;
using static Microsoft.Quic.MsQuic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicClientConnection : QuicPeerConnection
{
    private readonly QuicClientConfiguration _config;

    public ref readonly QuicClientConfiguration Configuration => ref _config;

    public unsafe QuicClientConnection(QuicClientConfiguration config)
        : base((config ?? throw new ArgumentNullException(nameof(config))).Registration, config.DatagramsAreReliable)
    {
        _config = config;

        QUIC_HANDLE* handle = null;

#if NET5_0_OR_GREATER
        var status = Registration.Table.ConnectionOpen
            (Registration.Handle, &NativeCallback, (void*)(IntPtr)GcHandle, &handle);
#else
        var status = Registration.Table.ConnectionOpen
            (Registration.Handle, NativeCallbackThunkPointer, (void*)(IntPtr)GcHandle, &handle);
#endif

        try
        {
            AssertSuccess(status);
        }
        catch
        {
            GcHandle.Free();
            throw;
        }

        Handle = handle;

        if (!DatagramsAreReliable) return;

        OutboundAcknowledgementStream = OpenUnidirectionalStream();
        OutboundAcknowledgementStream.Name = "Client Outbound Reliable Acknowledgement Stream";
        Debug.Assert(OutboundAcknowledgementStream.Id == 2);
    }


#if NET5_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    private static unsafe int NativeCallback(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* @event)
    {
        var @this = (QuicClientConnection)GCHandle.FromIntPtr((IntPtr)context).Target!;
        return @this.ManagedCallback(ref *@event);

    }

#if !NET5_0_OR_GREATER
    private unsafe delegate int NativeCallbackDelegate(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* @event);

    private static readonly unsafe NativeCallbackDelegate NativeCallbackThunk = NativeCallback;
    private static readonly unsafe delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int> NativeCallbackThunkPointer
        = (delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>)Marshal.GetFunctionPointerForDelegate(
            NativeCallbackThunk);
#endif

    private unsafe int ManagedCallback(ref QUIC_CONNECTION_EVENT @event)
    {
        switch (@event.Type)
        {
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_CONNECTED: {
                ref var typedEvent = ref @event.CONNECTED;

                IsResumed = typedEvent.SessionResumed != 0;

                var pAlpn = (IntPtr)typedEvent.NegotiatedAlpn;
                var alpnLength = typedEvent.NegotiatedAlpnLength;

                NegotiatedAlpn = SizedUtf8String.Create(alpnLength, span => {
                    fixed (void* pSpan = span)
                        Unsafe.CopyBlock(pSpan, (void*)pAlpn, alpnLength);
                });

                Interlocked.Exchange(ref RunState, 2);
                if (!ResumptionTicket.IsEmpty)
                    fixed (byte* pTicket = ResumptionTicket.Span)
                    {
                        Registration.Table.ConnectionSendResumptionTicket(Handle,
                            QUIC_SEND_RESUMPTION_FLAGS.QUIC_SEND_RESUMPTION_FLAG_FINAL,
                            (ushort)ResumptionTicket.Length, pTicket);
                    }
                else
                    Registration.Table.ConnectionSendResumptionTicket(Handle,
                        QUIC_SEND_RESUMPTION_FLAGS.QUIC_SEND_RESUMPTION_FLAG_FINAL,
                        0, null);

                OnConnected();

                Trace.TraceInformation(
                    $"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{NegotiatedAlpn={NegotiatedAlpn},IsResumed={IsResumed}}}");

                return 0;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE:
                Interlocked.Exchange(ref RunState, 0);
                GcHandle.Free();
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return 0;

            // client only
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_RESUMPTION_TICKET_RECEIVED: {
                ref var typedEvent = ref @event.RESUMPTION_TICKET_RECEIVED;
                var length = (int)typedEvent.ResumptionTicketLength;
                var resumptionTicket = new ReadOnlySpan<byte>(typedEvent.ResumptionTicket, length);
                _resumptionTicket = new(new byte[length]);
                resumptionTicket.CopyTo(_resumptionTicket.Span);
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{ResumptionTicket.Length={length}}}");
                OnResumptionTicketReceived();
                Interlocked.Exchange(ref RunState, 2);
                return QUIC_STATUS_SUCCESS;
            }
        }
        return DefaultManagedCallback(ref @event);
    }

    public event Utilities.EventHandler<QuicClientConnection>? Connected;

    private void OnConnected()
        => Connected?.Invoke(this);

    public override unsafe void Dispose()
        => Close();

    public unsafe void Start(SizedUtf8String name, ushort port)
    {
        if (Interlocked.CompareExchange(ref RunState, 1, 0) != 0)
            throw new InvalidOperationException("Already connecting.");

        var status = Registration.Table.ConnectionStart(Handle, _config.Handle,
            (ushort)sockaddr.AF_UNSPEC, name.Pointer, port);

        if (IsPending(status)) return;

        AssertSuccess(status);
    }

    public unsafe Task ConnectAsync(SizedUtf8String name, ushort port)
    {
        if (Interlocked.CompareExchange(ref RunState, 1, 0) != 0)
            throw new InvalidOperationException("Already connecting.");

        var status = Registration.Table.ConnectionStart(Handle, _config.Handle,
            (ushort)sockaddr.AF_UNSPEC, name.Pointer, port);

        if (IsSuccess(status))
            return Task.CompletedTask;
        if (IsPending(status))
        {
#if NETSTANDARD
            var tcs = new TaskCompletionSource<bool>();
            Connected += (_) => tcs.TrySetResult(true);
#else
            var tcs = new TaskCompletionSource();
            Connected += _ => tcs.TrySetResult();
#endif
            return tcs.Task;
        }
        if (IsFailure(status))
            throw new QuicException(status);

        throw new NotImplementedException("Unknown non-failure status",
            new QuicException(status));
    }

    protected override int DefaultCertificateReceivedStatus
        => QUIC_STATUS_SUCCESS;

    public override unsafe string ToString()
        => Name is null ? $"[QuicClientConnection 0x{(ulong)_handle:X}]" : $"[QuicClientConnection \"{Name}\" 0x{(ulong)_handle:X}]";
}
