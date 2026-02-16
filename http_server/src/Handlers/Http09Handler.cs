namespace http_server.Handlers;

public class Http09Handler :  BaseConnectionHandler
{
    protected override async Task HandleRequest(ConnectionContext context)
    {
        await Task.Delay(1);
    }
}