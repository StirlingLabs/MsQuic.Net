using JetBrains.Annotations;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public delegate void EventHandler<in TSender>(TSender sender);

[PublicAPI]
public delegate void EventHandler<in TSender, in T>(TSender sender, T arg);

#pragma warning disable CA1005
[PublicAPI]
public delegate void EventHandler<in TSender, in T1, in T2>
    (TSender sender, T1 arg1, T2 arg2);

[PublicAPI]
public delegate void EventHandler<in TSender, in T1, in T2, in T3>
    (TSender sender, T1 arg1, T2 arg2, T3 arg3);

[PublicAPI]
public delegate void EventHandler<in TSender, in T1, in T2, in T3, in T4>
    (TSender sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

[PublicAPI]
public delegate void EventHandler<in TSender, in T1, in T2, in T3, in T4, in T5>
    (TSender sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

[PublicAPI]
public delegate void EventHandler<in TSender, in T1, in T2, in T3, in T4, in T5, in T6>
    (TSender sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
#pragma warning restore CA1005
