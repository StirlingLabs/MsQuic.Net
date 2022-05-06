using System;
using JetBrains.Annotations;

namespace StirlingLabs.MsQuic.Bindings;

[PublicAPI]
public class QuicException : Exception
{
    public int Status { get; }

    public QuicException(int status)
        : this(MsQuic.GetNameForStatus(status) ?? status.ToString())
        => Status = status;

    private QuicException() { }

    private QuicException(string message, Exception innerException) : base(message, innerException) { }

    private QuicException(string message) : base(message) { }
}
