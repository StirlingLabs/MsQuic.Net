using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using StirlingLabs.MsQuic.Bindings;
using StirlingLabs.Utilities;
using MemoryMarshal = System.Runtime.InteropServices.MemoryMarshal;
using NativeMemory = StirlingLabs.Native.NativeMemory;

namespace StirlingLabs.MsQuic;

public abstract partial class QuicPeerConnection
{
    private ulong _reliableIdCounter = 1;
    private object _reliableDatagramAckLock = new();

    private object _reliableDatagramSentUnacknowledgedLock = new();
    private SortedList<ulong, IQuicDatagramReliable> _reliableDatagramsSentUnacknowledged = new();

    protected internal ulong GenerateReliableId()
#if NETSTANDARD
        => (ulong)Interlocked.Increment(ref Unsafe.As<ulong, long>(ref _reliableIdCounter)) - 1;
#else
        => Interlocked.Add(ref _reliableIdCounter, 1) - 1;
#endif

    public static TimeSpan ReliableDatagramAcknowledgementIntervalTime { get; set; } = TimeSpan.FromMilliseconds(1000.0 / 3);

    public static TimeSpan ReliableDatagramTrackAcknowledgements { get; set; } = TimeSpan.FromSeconds(60);

    public static TimeSpan ReliableDatagramRetransmitInterval { get; set; } = TimeSpan.FromMilliseconds(1000.0 / 3 * 2);

    protected internal static readonly unsafe QUIC_BUFFER* ZeroHeader = NativeMemory.New<QUIC_BUFFER>(
        pZeroHeader => {
            pZeroHeader->Buffer = NativeMemory.New<byte>();
            pZeroHeader->Length = 1;
        });

    public bool DatagramsAreReliable { get; private set; }

    private Interval? ReliableDatagramAcknowledgementInterval { get; set; }

    [SuppressMessage("Design", "CA1031", Justification = "Exceptions get aggregated")]
    internal static void ParseAckDatagram(Span<byte> data, Action<ulong> forEach)
    {

        do
        {
            var l = VarIntSqlite4.GetDecodedLength(data[0]);
            var ackRangeStart = VarIntSqlite4.Decode(data);
            data = data.Slice(l);

            l = VarIntSqlite4.GetDecodedLength(data[0]);
            var ackRangeEnd = VarIntSqlite4.Decode(data);
            data = data.Slice(l);

            if (ackRangeEnd == 0)
                ackRangeEnd = ackRangeStart;
            else
                Debug.Assert(ackRangeStart < ackRangeEnd);

            var exceptions = new ReadOnlyCollectionBuilder<Exception>();

            for (var i = ackRangeStart; i <= ackRangeEnd; ++i)
            {
                try
                {
                    forEach(i);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            switch (exceptions.Count)
            {
                case 1: throw exceptions[0];
                case > 0: throw new AggregateException(exceptions);
            }

        } while (data.Length != 0);
    }

    private void AcknowledgeReliableDatagramById(ulong i)
    {
        if (!_reliableDatagramsSentUnacknowledged.TryGetValue(i, out var dg))
            return;

        // ReSharper disable once RedundantAssignment
        var removed = _reliableDatagramsSentUnacknowledged.Remove(i);
        Debug.Assert(removed);

        dg.OnReliablyAcknowledged();
    }

    private void ParseAckDatagram(Span<byte> data)
    {
        Debug.Assert(Monitor.IsEntered(_reliableDatagramSentUnacknowledgedLock));
        ParseAckDatagram(data, AcknowledgeReliableDatagramById);
    }

    protected internal bool RemoveFromUnacknowledged(IQuicDatagramReliable dg)
    {
        if (dg is null) throw new ArgumentNullException(nameof(dg));

        lock (_reliableDatagramSentUnacknowledgedLock)
            return _reliableDatagramsSentUnacknowledged.Remove(dg.Id);
    }

    [SuppressMessage("Reliability", "CA2000", Justification = "Disposed only after finished sending")]
    private bool TrySendDatagramAcks(SortedSet<ulong> toAck)
    {
        Debug.Assert(Monitor.IsEntered(_reliableDatagramAckLock));

        if (OutboundAcknowledgementStream is null
            || OutboundAcknowledgementStream.Disposed) return false;

        if (toAck.Count <= 0) return false;

        var limit = _maxSendLength;
        var memOwner = MemoryPool<byte>.Shared.Rent(limit);

        var mem = memOwner.Memory;

        var span = mem.Span;

        var length = EncodeAcknowledgements(toAck, span.Slice(9));

        var headerSize = VarIntSqlite4.GetEncodedLength((ulong)length);

        mem = mem.Slice(9 - headerSize, (int)length + headerSize);

        span = mem.Span;

        // ReSharper disable once RedundantAssignment
        var headerSizeEncoded = VarIntSqlite4.Encode((ulong)length, span);
        Debug.Assert(headerSize == headerSizeEncoded);

        if (OutboundAcknowledgementStream.Disposed) return false;

        OutboundAcknowledgementStream!.SendAsync(mem,
            OutboundAcknowledgementStream.IsStarted
                ? QUIC_SEND_FLAGS.QUIC_SEND_FLAG_NONE
                : QUIC_SEND_FLAGS.QUIC_SEND_FLAG_START);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long EncodeAcknowledgements(SortedSet<ulong> acks, Span<byte> span)
    {
        // ReSharper disable once VariableHidesOuterVariable
        bool TryDequeue(out ulong ack)
        {
            ack = acks.Min;
            return acks.Remove(ack);
        }

        // bail if no acks
        if (!TryDequeue(out var ack)) return 0;

        // bail if too small
        if (span.Length < 18) return 0;

        ulong i = 0;
        ulong prevAck;
        long length = 0;
        // first ack
        {
            // always encode first ack
            var l = VarIntSqlite4.Encode(ack, span);
            length += l;
            span = span.Slice(l);
            prevAck = ack;
        }
        while (TryDequeue(out ack))
        {
            ++i;

            // skip contiguous acks
            if (ack == prevAck + 1)
            {
                ++prevAck;
                continue;
            }

            var l = VarIntSqlite4.Encode(prevAck, span);
            length += l;
            span = span.Slice(l);

            // bail if can't encode another range
            if (span.Length < 18)
            {
                // return unsent ack
                acks.Add(ack);
                return length;
            }

            l = VarIntSqlite4.Encode(ack, span);
            length += l;
            span = span.Slice(l);

            prevAck = ack;
        }

        // encode final ack
        {
            length += i == 0
                ? 1 // add a single zero byte
                : VarIntSqlite4.Encode(prevAck, span);
        }
        Debug.Assert(length >= 2);
        return length;
    }

    public static unsafe void CleanUpReliableDatagramBuffer(IQuicDatagramReliable dgr)
    {
        var buf = dgr.GetBuffer();

        if (buf == null)
            return;

        var pFirstBuf = &buf[0];

        if (pFirstBuf == ZeroHeader || pFirstBuf->Buffer == null)
            return;

        NativeMemory.Free(pFirstBuf->Buffer);
        pFirstBuf->Buffer = null;
        pFirstBuf->Length = 0;
    }

    private IMemoryOwner<byte>? _reliableAckPacketBufferOwner;
    private Memory<byte> _reliableAckPacketBuffer;

    private void WireUpInboundAcknowledgementStream()
    {
        Debug.Assert(InboundAcknowledgementStream is not null);
        InboundAcknowledgementStream!.Name = "Inbound Reliable Acknowledgement Stream";
        InboundAcknowledgementStream!.DataReceived += HandleInboundAcknowledgements;
    }
    private void HandleInboundAcknowledgements(QuicStream s)
    {
        lock (_reliableDatagramSentUnacknowledgedLock)
        {
            var len = (int)s.DataAvailable;
            Span<byte> newData = stackalloc byte[len];
            var got = s.Receive(newData);
            Debug.Assert(got == len);
            var prevBuffer = _reliableAckPacketBuffer;
            var savedNewDataAlready = false;
            try
            {
                if (prevBuffer.IsEmpty)
                    do
                    {
                        var frameHeaderSize = VarIntSqlite4.GetDecodedLength(newData[0]);
                        var frameSize = checked((int)VarIntSqlite4.Decode(newData));
                        Debug.Assert(frameSize >= 2);
                        var frame = newData.Slice(frameHeaderSize, frameSize);

                        ParseAckDatagram(frame);
                        newData = newData.Slice(frameHeaderSize + frameSize);
                    } while (!newData.IsEmpty);
                else
                {
                    // new size includes previous buffer
                    var prevSize = prevBuffer.Length;
                    var newSize = got + prevSize;
                    var owner = MemoryPool<byte>.Shared.Rent(newSize);
                    var buffer = owner.Memory;

                    // copy from previous and new buffers
                    prevBuffer.Span.CopyTo(buffer.Span);
                    newData.CopyTo(buffer.Span.Slice(prevSize));

                    // update current buffer and dispose of previous buffer
                    _reliableAckPacketBuffer = buffer.Slice(0, newSize);
                    var prevOwner = _reliableAckPacketBufferOwner;
                    _reliableAckPacketBufferOwner = owner;
                    prevOwner?.Dispose();
                    savedNewDataAlready = true;
                    var data = buffer.Span;

                    do
                    {
                        var frameHeaderSize = VarIntSqlite4.GetDecodedLength(data[0]);
                        var frameSize = checked((int)VarIntSqlite4.Decode(data));
                        Debug.Assert(frameSize >= 2);
                        var frame = data.Slice(frameHeaderSize, frameSize);
                        ParseAckDatagram(frame);
                        var offset = frameHeaderSize + frameSize;

                        _reliableAckPacketBuffer = buffer.Slice(offset);
                        //data = data.Slice(offset);

                        Debug.Assert(Unsafe.AreSame(ref MemoryMarshal.GetReference(data.Slice(offset)),
                            ref MemoryMarshal.GetReference(_reliableAckPacketBuffer.Span)));

                        data = _reliableAckPacketBuffer.Span;
                    } while (!_reliableAckPacketBuffer.IsEmpty);
                }
            }
            catch (ArgumentException)
            {
                if (prevBuffer.IsEmpty)
                {
                    // save to try parsing with next packet
                    _reliableAckPacketBufferOwner = MemoryPool<byte>.Shared.Rent(got);
                    _reliableAckPacketBuffer = _reliableAckPacketBufferOwner.Memory;
                    newData.CopyTo(prevBuffer.Span);
                    _reliableAckPacketBuffer = prevBuffer.Slice(0, got);
                }
                else
                {
                    if (savedNewDataAlready)
                        return;

                    // new size includes previous buffer
                    var prevSize = prevBuffer.Length;
                    var newSize = got + prevSize;
                    var owner = MemoryPool<byte>.Shared.Rent(newSize);
                    var buffer = owner.Memory;

                    // copy from previous and new buffers
                    prevBuffer.Span.CopyTo(buffer.Span);
                    newData.CopyTo(buffer.Span.Slice(prevSize));

                    // update current buffer and dispose of previous buffer
                    _reliableAckPacketBuffer = buffer.Slice(0, newSize);
                    var prevOwner = _reliableAckPacketBufferOwner;
                    _reliableAckPacketBufferOwner = owner;
                    prevOwner?.Dispose();
                }
                // save data for next packet
            }
        }
    }
}
