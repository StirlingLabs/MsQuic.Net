using JetBrains.Annotations;

namespace Microsoft.Quic;

[PublicAPI]
public abstract class MsQuicStatusBase
{
    public abstract string? GetNameForStatus(int status);

    public abstract int ResolveNameToStatus(string? name);

    public abstract bool IsSuccess(int status);

    public abstract bool IsPending(int status);

    public abstract bool IsContinue(int status);

    public abstract bool IsFailure(int status);
}
