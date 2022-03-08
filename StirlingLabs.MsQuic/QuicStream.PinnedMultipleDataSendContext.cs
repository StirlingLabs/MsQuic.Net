using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Quic;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

public partial class QuicStream
{
    [PublicAPI]
    private sealed class PinnedMultipleDataSendContext : SendContext
    {
        public MemoryHandle[] DataHandles;
        public readonly unsafe QUIC_BUFFER* QuicBuffer;

        public unsafe PinnedMultipleDataSendContext(MemoryHandle[] dataHandles, QUIC_BUFFER* quicBuffer,
            TaskCompletionSource<bool> taskCompletionSource)
            : base(taskCompletionSource)
        {
            if (quicBuffer == null) throw new ArgumentNullException(nameof(quicBuffer));
            Trace.TraceInformation($"Constructed {this}");
            DataHandles = dataHandles;
            QuicBuffer = quicBuffer;
        }

        public override unsafe void Dispose()
        {
            Trace.TraceInformation($"Disposing {this}");
            foreach (var handle in DataHandles)
                handle.Dispose();
            NativeMemory.Free(QuicBuffer);
        }

        public override unsafe string ToString()
            => $"[PinnedMultipleDataSendContext 0x{(long)(nuint)QuicBuffer:X}]";
    }
}
