namespace http.Handlers;

public interface IConnectionHandler
{
    Task Accept(ConnectionContext context);
}