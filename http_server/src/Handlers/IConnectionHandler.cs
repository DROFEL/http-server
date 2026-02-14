namespace http_server.Handlers;

public interface IConnectionHandler
{
    Task Accept(ConnectionContext context);
}