using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Quic
{
    [PublicAPI]
    public readonly partial struct sockaddr
    {
        public static readonly nuint SizeOfSockAddrIn
            = (nuint)new IPEndPoint(IPAddress.Any, 0).Serialize().Size;

        public static readonly nuint SizeOfSockAddrIn6
            = (nuint)new IPEndPoint(IPAddress.IPv6Any, 0).Serialize().Size;

        public static readonly nuint MaxSizeOfSockAddr
            = SizeOfSockAddrIn6 > SizeOfSockAddrIn ? SizeOfSockAddrIn6 : SizeOfSockAddrIn;

        private static readonly IPEndPoint IpEndPointInstance = new(0, 0);

        private static readonly IPEndPoint IpEndPointV6Instance = new(IPAddress.IPv6None, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Serialize(IPEndPoint value, Span<byte> bytes)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var bytesLength = (nuint)bytes.Length;

            switch (value.AddressFamily)
            {
                case AddressFamily.InterNetwork: {
                    if (bytesLength < SizeOfSockAddrIn)
                        throw new ArgumentException("Not enough bytes.", nameof(bytes));
                    break;
                }
                case AddressFamily.InterNetworkV6: {
                    if (bytesLength < SizeOfSockAddrIn6)
                        throw new ArgumentException("Not enough bytes.", nameof(bytes));
                    break;
                }
                default: throw new NotImplementedException("Address family not implemented.");
            }

            var sa = value.Serialize();

            var saSize = sa.Size;

            fixed (byte* data = bytes)
            {
                for (var i = 0; i < saSize; ++i)
                    data[i] = sa[i];
            }
        }

        public static unsafe IPEndPoint Read(sockaddr* address)
        {
            var af = (AddressFamily)(*(short*)address);
            var sa = new SocketAddress(af);
            var saSize = (uint)sa.Size;
            var data = (byte*)address;
            for (var i = 0; i < saSize; ++i)
                sa[i] = data[i];
            if (af == AddressFamily.InterNetworkV6)
                return (IPEndPoint)IpEndPointV6Instance.Create(sa);
            return (IPEndPoint)IpEndPointInstance.Create(sa);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe sockaddr* New(IPEndPoint value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var sa = value.Serialize();

            var saSize = sa.Size;

            var data = (byte*)Marshal.AllocHGlobal(saSize);

            for (var i = 0; i < saSize; ++i)
                data[i] = sa[i];

            return (sockaddr*)data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Free(sockaddr* sa)
            => Marshal.FreeHGlobal((IntPtr)sa);
    }
}
