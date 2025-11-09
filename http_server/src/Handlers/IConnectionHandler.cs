namespace http.Handlers;

public interface IConnectionHandler
{
    Task Accept(Stream stream, CancellationToken ct = default);
}