using System;
using JetBrains.Annotations;

namespace StirlingLabs.MsQuic;

[PublicAPI]
public delegate void SpanEventHandler<in TSender, T>(TSender sender, Span<T> data);
