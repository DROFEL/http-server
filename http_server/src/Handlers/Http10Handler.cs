using System.IO.Pipelines;

namespace http.Handlers;

public class Http10Handler :  BaseConnectionHandler
{
    protected override async Task HandleRequest(ConnectionContext context)
    {
        await Task.Delay(1);
    }
}