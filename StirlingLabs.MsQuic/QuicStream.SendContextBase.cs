using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace StirlingLabs.MsQuic;

public partial class QuicStream
{
    [PublicAPI]
    internal abstract class SendContext
        : IDisposable
    {
        private SendContext() => throw new NotSupportedException();

        protected SendContext(TaskCompletionSource<bool> taskCompletionSource)
            => TaskCompletionSource = taskCompletionSource;

        public TaskCompletionSource<bool> TaskCompletionSource { get; set; }

        public abstract unsafe void Dispose();
    }
}
