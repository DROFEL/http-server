namespace http_server;

public interface IConnectionHandler : IAsyncDisposable
{
    public Task StartAsync(CancellationToken ct);
    public Task StopAsync();
}