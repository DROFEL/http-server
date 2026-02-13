using System.IO.Pipelines;
using http_server.helpers;

namespace http.Handlers;

public abstract class BaseConnectionHandler: IConnectionHandler
{
    private static int _requestCount = 0;

    public async Task Accept(ConnectionContext context)
    {
        Interlocked.Increment(ref _requestCount);
        await HandleRequest(context);
    }

    protected abstract Task HandleRequest(ConnectionContext context);
    public static int RequestCount => Volatile.Read(ref _requestCount);
}