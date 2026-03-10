namespace http_server;

public interface ITcpConnectionAccepter: IDisposable
{
    void Start();
    void Stop();
    ValueTask<Stream> AcceptStreamAsync(CancellationToken cancellationToken = default);
}