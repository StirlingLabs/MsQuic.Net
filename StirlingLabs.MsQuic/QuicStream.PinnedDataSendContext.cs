using System;
using System.Buffers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Quic;
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
            DataHandle = dataHandle;
            QuicBuffer = quicBuffer;
        }

        public override unsafe void Dispose()
        {
            DataHandle.Dispose();
            NativeMemory.Free(QuicBuffer);
        }
    }
}
