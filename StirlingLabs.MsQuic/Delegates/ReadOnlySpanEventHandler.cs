using System;
using JetBrains.Annotations;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public delegate void ReadOnlySpanEventHandler<in TSender, T>(TSender sender, ReadOnlySpan<T> data);
