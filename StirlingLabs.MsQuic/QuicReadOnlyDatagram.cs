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
[SuppressMessage("Design", "CA1063", Justification = "It's fine")]
public abstract class QuicReadOnlyDatagram : IQuicReadOnlyDatagram
{
    private QUIC_SEND_FLAGS _flags;
    private QUIC_DATAGRAM_SEND_STATE _state;
    private TaskCompletionSource<bool> _tcsSent = new();
    private TaskCompletionSource<bool> _tcsAcknowledged = new();
    protected QuicReadOnlyDatagram(QuicPeerConnection connection, QUIC_DATAGRAM_SEND_STATE state)
    {
        GcHandle = GCHandle.Alloc(this);
        Connection = connection;
        State = state;
    }
    protected const QUIC_DATAGRAM_SEND_STATE Unknown = (QUIC_DATAGRAM_SEND_STATE)(-1);
    public QuicPeerConnection Connection { get; }
    public GCHandle GcHandle { get; }

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

    public QUIC_SEND_FLAGS Flags
    {
        get => _flags;
        set {
            if (State != Unknown)
                throw new InvalidOperationException("You can't update the send flags after the datagram is sent.");

            _flags = value;
        }
    }

    public bool IsSent { get; private set; }
    public bool IsCanceled { get; private set; }
    public bool IsAcknowledged { get; private set; }
    public bool IsLost { get; private set; }
    public bool IsDiscarded { get; private set; }

    public abstract unsafe QUIC_BUFFER* GetBuffer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanCreate(QuicPeerConnection connection, ReadOnlyMemory<byte> data)
        => CanCreate(connection, data.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanCreate(QuicPeerConnection connection, long size)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        return CanCreateInternal(connection, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static bool CanCreateInternal(QuicPeerConnection connection, long size)
        => size < connection.MaxSendLength;

    public static bool TryCreate(QuicPeerConnection connection, ReadOnlyMemory<byte> data,
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out QuicReadOnlyDatagram? dg)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (connection is null
            || !CanCreateInternal(connection, data.Length))
        {
            dg = null;
            return false;
        }

        dg = connection.DatagramsAreReliable
            ? new QuicReadOnlyDatagramManagedMemoryReliable(connection, data)
            : new QuicReadOnlyDatagramManagedMemory(connection, data);
        return true;
    }

    public static QuicReadOnlyDatagram Create(QuicPeerConnection connection, ReadOnlyMemory<byte> data)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        if (!CanCreateInternal(connection, data.Length))
            throw new ArgumentOutOfRangeException(nameof(data), "Message size too large to fit into a datagram.");

        return connection.DatagramsAreReliable
            ? new QuicReadOnlyDatagramManagedMemoryReliable(connection, data)
            : new QuicReadOnlyDatagramManagedMemory(connection, data);
    }
    public static bool TryCreate(QuicPeerConnection connection, IMemoryOwner<byte> owner, ReadOnlyMemory<byte> data,
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out QuicReadOnlyDatagram? dg)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (connection is null
            || !CanCreateInternal(connection, data.Length))
        {
            dg = null;
            return false;
        }

        dg = connection.DatagramsAreReliable
            ? new QuicReadOnlyDatagramOwnedManagedMemoryReliable(connection, owner, data)
            : new QuicReadOnlyDatagramOwnedManagedMemory(connection, owner, data);
        return true;
    }
    public static QuicReadOnlyDatagram Create(QuicPeerConnection connection, IMemoryOwner<byte> owner, ReadOnlyMemory<byte> data)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        if (!CanCreateInternal(connection, data.Length))
            throw new ArgumentOutOfRangeException(nameof(data), "Message size too large to fit into a datagram.");

        return connection.DatagramsAreReliable
            ? new QuicReadOnlyDatagramOwnedManagedMemoryReliable(connection, owner, data)
            : new QuicReadOnlyDatagramOwnedManagedMemory(connection, owner, data);
    }

    public Task WaitForSentAsync() => _tcsSent.Task;

    public Task WaitForAcknowledgementAsync() => _tcsAcknowledged.Task;

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
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_UNKNOWN:
            case QUIC_DATAGRAM_SEND_STATE.QUIC_DATAGRAM_SEND_SENT:
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
    public event EventHandler<IQuicReadOnlyDatagram, QUIC_DATAGRAM_SEND_STATE>? StateChanged;

    protected internal void OnStateChanged(QUIC_DATAGRAM_SEND_STATE state)
        => StateChanged?.Invoke(this, state);

    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public event EventHandler<IQuicReadOnlyDatagram, ExceptionDispatchInfo>? UnobservedException;

    protected internal void OnUnobservedException(ExceptionDispatchInfo arg)
    {
        Debug.Assert(arg != null);
        Trace.TraceError($"{LogTimeStamp.ElapsedSeconds:F6} {this} {arg!.SourceException}");
        UnobservedException?.Invoke(this, arg);
    }
}
