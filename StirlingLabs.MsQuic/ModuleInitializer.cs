using System.Runtime.CompilerServices;

namespace StirlingLabs.MsQuic;

#if NET5_0_OR_GREATER
internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
        => LogTimeStamp.Init();
}
#endif