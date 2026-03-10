using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace http_server.Tests.Mocks;

public class MockTcpAccepterAdapter : ITcpConnectionAccepter
{
    private readonly Channel<Stream> _channel = Channel.CreateUnbounded<Stream>();

    public void EnqueueStream(Stream stream)
    {
        _channel.Writer.TryWrite(stream);
    }

    public void Dispose()
    {
        _channel.Writer.TryComplete();
    }

    public void Start()
    {
    }

    public void Stop()
    {
        _channel.Writer.TryComplete();
    }

    public async ValueTask<Stream> AcceptStreamAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}