namespace StirlingLabs.MsQuic;

public interface IQuicDatagram : IQuicReadOnlyDatagram
{
    bool WipeWhenFinished { get; set; }
}
