using System.IO.Pipelines;

namespace http.Handlers;

public class Http10Handler :  BaseConnectionHandler
{
    protected override async Task HandleRequest(PipeReader reader, PipeWriter writer, CancellationToken cancellationToken)
    {
        await Task.Delay(1);
    }
}