namespace http_server.Handlers;

public interface IHttpVersionHandler : IAsyncDisposable, IDisposable
{
    Task HandleAsync(CancellationToken ct);
}