using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace StirlingLabs.MsQuic.Bindings;

public static class MsQuicStatus
{
    public static MsQuicStatusBase GetPlatformSpecificImplementation()
    {
        AssemblyName asmName
            = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new("StirlingLabs.MsQuic.Bindings.MsQuic.Windows")
                : new("StirlingLabs.MsQuic.Bindings.Posix");
        var asm = Assembly.Load(asmName);
        var type = asm.GetType("StirlingLabs.MsQuic.Bindings.MsQuicStatusImpl")!;
        return (MsQuicStatusBase)Activator.CreateInstance(type)!;
    }
}
