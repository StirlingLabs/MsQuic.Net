using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;
using static Microsoft.Quic.MsQuic;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public sealed class QuicServerConnection : QuicPeerConnection
{
    private readonly QuicServerConfiguration _config;

    public unsafe QuicServerConnection(QuicServerConfiguration config, QUIC_HANDLE* handle, QUIC_NEW_CONNECTION_INFO* info)
        : base((config ?? throw new ArgumentNullException(nameof(config))).Registration, config.DatagramsAreReliable)
    {
        Handle = handle;
        _config = config;

        var pAlpn = (IntPtr)info->NegotiatedAlpn;
        var alpnLength = info->NegotiatedAlpnLength;

        NegotiatedAlpn = SizedUtf8String.Create(alpnLength, span => {
            fixed (void* pSpan = span)
                Unsafe.CopyBlock(pSpan, (void*)pAlpn, alpnLength);
        });

        LocalEndPoint = ((sockaddr*)info->LocalAddress)->ToEndPoint();
        RemoteEndPoint = ((sockaddr*)info->RemoteAddress)->ToEndPoint();

        var pSn = (IntPtr)info->ServerName;
        var snLength = info->ServerNameLength;

        ServerName = SizedUtf8String.Create(snLength, span => {
            fixed (void* pSpan = span)
                Unsafe.CopyBlock(pSpan, (void*)pSn, snLength);
        });

        //QuicVersion = info->QuicVersion;

#if NET5_0_OR_GREATER
        delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int> cb = &NativeCallback;
#else
        var cb = NativeCallbackThunkPointer;
#endif
        Registration.Table.SetCallbackHandler(handle, cb, (void*)(IntPtr)GcHandle);

        if (!DatagramsAreReliable) return;

        OutboundAcknowledgementStream = OpenUnidirectionalStream();
        OutboundAcknowledgementStream.Name = "Server Outbound Reliable Acknowledgement Stream";
        OutboundAcknowledgementStream.WaitForStartCompleteAsync()
            .ContinueWith(t => {
                AssertSuccess(t.Result);
                Debug.Assert(OutboundAcknowledgementStream.Id == 3);
            });
    }

#if NET5_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#endif
    private static unsafe int NativeCallback(QUIC_HANDLE* handle, void* context, QUIC_CONNECTION_EVENT* @event)
    {
        var @this = (QuicServerConnection)GCHandle.FromIntPtr((IntPtr)context).Target!;
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

                var enabled = false;
                {
                    uint l = sizeof(bool);
                    Registration.Table.GetParam(Handle, QUIC_PARAM_CONN_DATAGRAM_SEND_ENABLED, &l, &enabled);
                }
                DatagramsAllowed = enabled;
                MaxSendLength = 1248;
                
                
                return QUIC_STATUS_SUCCESS;
            }

            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE: {
                //Close();
                GcHandle.Free();
                //RunState ?
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type}");
                return QUIC_STATUS_SUCCESS;
            }

            // server only
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_RESUMED: {
                ref var typedEvent = ref @event.RESUMED;
                var length = (int)typedEvent.ResumptionStateLength;
                var resumptionState = new ReadOnlySpan<byte>(typedEvent.ResumptionState, length);
                _resumptionState = new(new byte[length]);
                resumptionState.CopyTo(_resumptionState.Span);
                Trace.TraceInformation($"{LogTimeStamp.ElapsedSeconds:F6} {this} {@event.Type} {{ResumptionState.Length={length}}}");
                Interlocked.Exchange(ref RunState, 2);

                /*
                [SuppressMessage("Maintainability", "CA1508")]
                void FetchNegotiatedAlpn()
                {
                    uint bufSize = 0;
                    var pBufSize = (uint*)Unsafe.AsPointer(ref bufSize);

                    var status = Registration.Table.GetParam(Handle,
                        QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_TLS,
                        QUIC_PARAM_TLS_NEGOTIATED_ALPN,
                        pBufSize,
                        pBufSize);
                    if (status == QUIC_STATUS_BUFFER_TOO_SMALL)
                        Debug.Assert(bufSize != 0);
                    else
                        AssertSuccess(status);
                    if (bufSize == 0)
                        return;
                    Span<byte> buf = stackalloc byte[(int)bufSize];
                    fixed (byte* pBuf = buf)
                    {
                        var pBufPtr = (IntPtr)pBuf;
                        status = Registration.Table.GetParam(Handle,
                            QUIC_PARAM_LEVEL.QUIC_PARAM_LEVEL_TLS,
                            QUIC_PARAM_TLS_NEGOTIATED_ALPN,
                            pBufSize,
                            pBuf);
                        AssertSuccess(status);
                        NegotiatedAlpn = SizedUtf8String.Create(bufSize, span => {
                            var sBuf = new Span<sbyte>((void*)pBufPtr, (int)*pBufSize);
                            sBuf.CopyTo(span);
                        });
                    }
                }

                FetchNegotiatedAlpn();*/

                OnConnected();

                return QUIC_STATUS_SUCCESS;
            }

        }
        return DefaultManagedCallback(ref @event);

    }

    public override unsafe void Dispose()
        => Close();

    public event Utilities.EventHandler<QuicServerConnection>? Connected;

    private void OnConnected()
    {
        Interlocked.Exchange(ref RunState, 1);
        Connected?.Invoke(this);
    }

    protected override int DefaultCertificateReceivedStatus
        => QUIC_STATUS_BAD_CERTIFICATE;

    public override unsafe string ToString()
        => Name is null ? $"[QuicServerConnection 0x{(ulong)Handle:X}]" : $"[QuicServerConnection \"{Name}\" 0x{(ulong)Handle:X}]";
}
