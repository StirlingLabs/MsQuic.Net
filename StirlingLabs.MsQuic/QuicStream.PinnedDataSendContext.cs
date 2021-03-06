using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StirlingLabs.MsQuic.Bindings;
using StirlingLabs.Native;

namespace StirlingLabs.MsQuic;

public partial class QuicStream
{
    [PublicAPI]
    private sealed class PinnedDataSendContext : SendContext
    {
        public MemoryHandle DataHandle;
        public readonly unsafe QUIC_BUFFER* QuicBuffer;

        public unsafe PinnedDataSendContext(MemoryHandle dataHandle, QUIC_BUFFER* quicBuffer, TaskCompletionSource<bool> taskCompletionSource)
            : base(taskCompletionSource)
        {
            if (quicBuffer == null) throw new ArgumentNullException(nameof(quicBuffer));
            Trace.TraceInformation($"Constructed {this}");
            DataHandle = dataHandle;
            QuicBuffer = quicBuffer;
        }

        public override unsafe void Dispose()
        {
            Trace.TraceInformation($"Disposing {this}");
            DataHandle.Dispose();
            NativeMemory.Free(QuicBuffer);
        }

        public override unsafe string ToString()
            => $"[PinnedDataSendContext 0x{(long)(nuint)QuicBuffer:X}]";
    }
}
