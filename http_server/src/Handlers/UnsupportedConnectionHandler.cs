namespace http_server.Handlers;

public class UnsupportedConnectionHandler : IConnectionHandler
{
    public Task Accept(ConnectionContext connectionContext)
    {
        return Task.CompletedTask;
    }
}