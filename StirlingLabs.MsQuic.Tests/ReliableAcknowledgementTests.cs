using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using StirlingLabs.Utilities;
using StirlingLabs.Utilities.Assertions;

namespace StirlingLabs.MsQuic.Tests;

[Order(2)]
public class ReliableAcknowledgementTests
{
    [Test]
    public void EncodingTest1()
    {
        Span<byte> bytes = stackalloc byte[1428];
        var acks = new SortedSet<ulong>();
        for (ulong i = 0; i <= 10; ++i) acks.Add(i);
        var l = QuicPeerConnection.EncodeAcknowledgements(acks, bytes);

        Assert.AreEqual(2, l);
        BigSpanAssert.AreEqual((ReadOnlyBigSpan<byte>)new byte[]
                { 0, 10 },
            bytes.Slice(0, (int)l));
    }
    [Test]
    public void EncodingTest2()
    {
        Span<byte> bytes = stackalloc byte[1428];
        var acks = new SortedSet<ulong>();
        for (ulong i = 0; i <= 1; ++i) acks.Add(i);
        for (ulong i = 3; i <= 4; ++i) acks.Add(i);
        var l = QuicPeerConnection.EncodeAcknowledgements(acks, bytes);

        Assert.AreEqual(4, l);
        BigSpanAssert.AreEqual((ReadOnlyBigSpan<byte>)new byte[]
                { 0, 1, 3, 4 },
            bytes.Slice(0, (int)l));
    }
    [Test]
    public void EncodingTest3()
    {
        Span<byte> bytes = stackalloc byte[1428];
        var acks = new SortedSet<ulong>();
        for (ulong i = 0; i <= 2; ++i) acks.Add(i);
        for (ulong i = 4; i <= 9; ++i) acks.Add(i);
        for (ulong i = 12; i <= 20; ++i) acks.Add(i);
        var l = QuicPeerConnection.EncodeAcknowledgements(acks, bytes);

        Assert.AreEqual(6, l);
        BigSpanAssert.AreEqual((ReadOnlyBigSpan<byte>)new byte[]
                { 0, 2, 4, 9, 12, 20 },
            bytes.Slice(0, (int)l));
    }
    [Test]
    public void EncodingTestTooShort()
    {
        Span<byte> bytes = stackalloc byte[18];
        var acks = new SortedSet<ulong>();
        for (ulong i = 0; i <= 1; ++i) acks.Add(i);
        for (ulong i = 3; i <= 4; ++i) acks.Add(i);
        var l = QuicPeerConnection.EncodeAcknowledgements(acks, bytes);

        Assert.AreEqual(2, l);
        BigSpanAssert.AreEqual((ReadOnlyBigSpan<byte>)new byte[]
                { 0, 1 },
            bytes.Slice(0, (int)l));

        l = QuicPeerConnection.EncodeAcknowledgements(acks, bytes);

        Assert.AreEqual(2, l);
        BigSpanAssert.AreEqual((ReadOnlyBigSpan<byte>)new byte[]
                { 3, 4 },
            bytes.Slice(0, (int)l));
    }

    [Test]
    public void DecodingTest()
    {
        Span<byte> bytes = stackalloc byte[1428];
        var acks = new SortedSet<ulong>();
        for (ulong i = 0; i <= 2; ++i) acks.Add(i);
        for (ulong i = 4; i <= 9; ++i) acks.Add(i);
        for (ulong i = 12; i <= 20; ++i) acks.Add(i);
        var acksCheck = new SortedSet<ulong>(acks);
        var l = QuicPeerConnection.EncodeAcknowledgements(acks, bytes);
        var span = bytes.Slice(0, (int)l);

        QuicPeerConnection.ParseAckDatagram(span, ack => {
            Assert.True(acksCheck.Remove(ack));
        });

        CollectionAssert.IsEmpty(acksCheck);
    }
}
