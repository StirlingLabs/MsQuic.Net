using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Quic;

public static class MsQuicStatus
{
    public static MsQuicStatusBase GetPlatformSpecificImplementation()
    {
        AssemblyName asmName
            = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new("Microsoft.Quic.MsQuic.Windows")
                : new("Microsoft.Quic.MsQuic.Posix");
        var asm = Assembly.Load(asmName);
        var type = asm.GetType("Microsoft.Quic.MsQuicStatusImpl")!;
        return (MsQuicStatusBase)Activator.CreateInstance(type)!;
    }
}
