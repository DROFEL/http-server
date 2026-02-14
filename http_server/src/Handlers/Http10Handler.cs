using System.IO.Pipelines;
using http;

namespace http_server.Handlers;

public class Http10Handler :  BaseConnectionHandler
{
    protected override async Task HandleRequest(ConnectionContext context)
    {
        await Task.Delay(1);
    }
}