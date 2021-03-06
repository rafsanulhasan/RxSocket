﻿using Xunit;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Xunit.Abstractions;

namespace RxSockets.xUnitTests
{
    public class RxSocket_Server_Test : TestBase
    {
        public RxSocket_Server_Test(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void T01_Invalid_EndPoint()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("111.111.111.111"), 1111);
            Assert.Throws<SocketException>(() => RxSocketServer.CreateOnEndPoint(endPoint, SocketServerLogger));
        }

        [Fact]
        public async Task T02_Accept_Success()
        {
            var server = RxSocketServer.Create(SocketServerLogger);
            var endPoint = server.IPEndPoint;

            var acceptTask = server.AcceptObservable.FirstAsync().ToTask();

            var clientSocket = Utilities.CreateSocket();
            clientSocket.Connect(endPoint);

            var acceptedSocket = await acceptTask;

            Assert.True(clientSocket.Connected && acceptedSocket.Connected);

            clientSocket.Disconnect(false);
            await server.DisposeAsync();
        }

        [Fact]
        public async Task T03_Disconnect_Before_Accept()
        {
            var server = RxSocketServer.Create(SocketServerLogger);
            await server.DisposeAsync();
            await Assert.ThrowsAsync<ObjectDisposedException> (async () => await server.AcceptObservable.LastOrDefaultAsync());
        }

        [Fact]
        public async Task T04_Disconnect_While_Accept()
        {
            var server = RxSocketServer.Create(SocketServerLogger);
            var acceptTask = server.AcceptObservable.LastAsync().ToTask();
            await server.DisposeAsync();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await acceptTask);
        }
    }
}
