namespace http.Handlers;

public class UnsupportedConnectionHandler : IConnectionHandler
{
    public Task Accept(Stream stream, CancellationToken ct = default)
    {
        stream.Close();
        return Task.CompletedTask;
    }
}