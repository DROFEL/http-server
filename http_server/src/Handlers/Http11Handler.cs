using System.IO.Pipelines;

namespace http.Handlers;

public class Http11Handler : BaseConnectionHandler
{
    protected override async Task HandleRequest(ConnectionContext context)
    {
        await Task.Delay(1);
    }
}