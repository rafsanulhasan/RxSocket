﻿namespace RxSockets;

public sealed class NullRxSocketClient : IRxSocketClient
{
    public static IRxSocketClient Instance { get; } = new NullRxSocketClient();
    private NullRxSocketClient() { }
    public EndPoint RemoteEndPoint => throw new InvalidOperationException();
    public bool Connected { get; }
    public int Send(ReadOnlySpan<byte> buffer) => throw new InvalidOperationException();
    public IAsyncEnumerable<byte> ReceiveAllAsync => throw new InvalidOperationException();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
