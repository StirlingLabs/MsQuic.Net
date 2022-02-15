using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Utilities;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public abstract class QuicDatagram
    : IDisposable
{
    protected const QUIC_DATAGRAM_SEND_STATE Unknown = (QUIC_DATAGRAM_SEND_STATE)(-1);
    private QUIC_SEND_FLAGS _flags;
    private QUIC_DATAGRAM_SEND_STATE _state;
    protected QuicPeerConnection Connection { get; }
    internal GCHandle GcHandle { get; }

    private TaskCompletionSource<bool> _tcsSent = new();
    private TaskCompletionSource<bool> _tcsAcknowledged = new();

    internal abstract unsafe QUIC_BUFFER* GetBuffer();

    protected QuicDatagram(QuicPeerConnection connection, QUIC_DATAGRAM_SEND_STATE state)
    {
        GcHandle = GCHandle.Alloc(this);
        Connection = connection;
        State = state;
    }

    public static QuicDatagram Create(QuicPeerConnection connection, Memory<byte> data)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        return connection.DatagramsAreReliable
            ? new QuicDatagramManagedMemoryReliable(connection, data)
            : new QuicDatagramManagedMemory(connection, data);
    }
    public static QuicDatagram Create(QuicPeerConnection connection, IMemoryOwner<byte> owner, Memory<byte> data)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        return connection.DatagramsAreReliable
            ? new QuicDatagramOwnedManagedMemoryReliable(connection, owner, data)
            : new QuicDatagramOwnedManagedMemory(connection, owner, data);
    }
    // 
    public static unsafe QuicDatagram Create(QuicPeerConnection connection, byte* pExternalMemStart, uint externalMemLength, Action<IntPtr> freeCallback)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        return connection.DatagramsAreReliable
            ? new QuicDatagramExternalMemoryReliable(connection, pExternalMemStart, externalMemLength, freeCallback)
            : new QuicDatagramExternalMemory(connection, pExternalMemStart, externalMemLength, freeCallback);
    }

    public Task WaitForSentAsync() => _tcsSent.Task;

    public Task WaitForAcknowledgementAsync() => _tcsAcknowledged.Task;

    public QUIC_DATAGRAM_SEND_STATE State
    {
        get => _state;
        set {
            _state = value;
            switch (value)
            {
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_SENT:
                    IsSent = true;
                    _tcsSent.TrySetResult(true);
                    break;
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_ACKNOWLEDGED_SPURIOUS:
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_ACKNOWLEDGED:
                    IsAcknowledged = true;
                    IsLost = false;
                    IsDiscarded = false;
                    _tcsSent.TrySetResult(true);
                    _tcsAcknowledged.TrySetResult(true);
                    break;
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_LOST_SUSPECT:
                    IsLost = true;
                    _tcsAcknowledged.TrySetException(new TimeoutException("Datagram suspected lost."));
                    break;
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_LOST_DISCARDED:
                    IsLost = true;
                    IsDiscarded = true;
                    _tcsAcknowledged.TrySetException(new Exception("Datagram lost and discarded."));
                    break;
                case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_CANCELED:
                    IsCanceled = true;
                    _tcsSent.TrySetCanceled();
                    _tcsAcknowledged.TrySetCanceled();
                    break;
            }
            OnStateChanged(value);
        }
    }

    public bool WipeWhenFinished { get; set; }

    public QUIC_SEND_FLAGS Flags
    {
        get => _flags;
        set {
            if (State != Unknown)
                throw new InvalidOperationException("You can't update the send flags after the datagram is sent.");

            _flags = value;
        }
    }

    public bool Send()
    {
        if (State != Unknown)
            return false;

        Connection.SendDatagram(this);
        return true;
    }

    public bool RetrySend()
    {
        switch (State)
        {
            default:
            case Unknown:
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_SENT: // 0
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_CANCELED:
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_ACKNOWLEDGED:
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_ACKNOWLEDGED_SPURIOUS:
                return false;
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_LOST_SUSPECT:
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_LOST_DISCARDED:
                Connection.SendDatagram(this);
                return true;
        }
    }

    [SuppressMessage("Design", "CA1063", Justification = "It's fine")]
    public virtual void Dispose()
    {
        _tcsSent.TrySetCanceled();
        _tcsAcknowledged.TrySetCanceled();
        GcHandle.Free();

        GC.SuppressFinalize(this);
    }

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicDatagram, QUIC_DATAGRAM_SEND_STATE>? StateChanged;

    private void OnStateChanged(QUIC_DATAGRAM_SEND_STATE state)
        => StateChanged?.Invoke(this, state);

    public bool IsSent { get; private set; }
    public bool IsCanceled { get; private set; }
    public bool IsAcknowledged { get; private set; }
    public bool IsLost { get; private set; }
    public bool IsDiscarded { get; private set; }


    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<QuicDatagram, ExceptionDispatchInfo>? UnobservedException;

    protected void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{LogTimeStamp.ElapsedSeconds:F6} {this} {arg.SourceException}");
        UnobservedException?.Invoke(this, arg);
    }
}
