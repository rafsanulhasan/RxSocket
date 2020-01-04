﻿using Xunit;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Xunit.Abstractions;
using System.Reactive.Concurrency;
using System.Threading;
using System.IO;
using System.Linq;

namespace RxSockets.Tests
{
    public class RxSocketClientServerTest : TestBase
    {
        public RxSocketClientServerTest(ITestOutputHelper output) : base(output)  {}

        [Fact]
        public async Task T01_Handshake()
        {
            NewThreadScheduler.Default.Schedule(async () =>
            {
                var server = IPEndPoint.CreateRxSocketServer(SocketServerLogger);
                var accept = await server.AcceptObservable.FirstAsync();

                var message1 = await ConversionsEx.ReadString(accept.ReadAsync);
                Assert.Equal("API", message1);

                var message2 = await ConversionsWithLengthPrefixEx.FromBytesWithLengthPrefix(accept.ReadAsync);
                Assert.Equal("HelloFromClient", message2.Single());

                new[] { "HelloFromServer" }.ToByteArrayWithLengthPrefix().SendFrom(accept);

                await server.DisposeAsync();
            });

            var client = await IPEndPoint.ConnectRxSocketClientAsync();

            // Send only the first message without prefix.
            "API".ToByteArray().SendFrom(client);

            // Start sending and receiving messages with an int32 message length prefix (UseV100Plus).
            new[] { "HelloFromClient" }.ToByteArrayWithLengthPrefix().SendFrom(client);

            var message3 = await ConversionsWithLengthPrefixEx.FromBytesWithLengthPrefix(client.ReadAsync);
            Assert.Equal("HelloFromServer", message3.Single());

            await client.DisposeAsync();
        }
    }
}