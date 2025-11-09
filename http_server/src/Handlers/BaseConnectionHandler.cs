using System.IO.Pipelines;
using http_server.helpers;

namespace http.Handlers;

public abstract class BaseConnectionHandler: IConnectionHandler
{
    private static int _requestCount = 0;

    public async Task Accept(Stream stream, CancellationToken ct = default)
    {
        var reader = PipeReader.Create(stream);
        var writer = PipeWriter.Create(stream);
        Interlocked.Increment(ref _requestCount);
        await HandleRequest(reader, writer, ct);
    }

    protected abstract Task HandleRequest(PipeReader reader, PipeWriter writer, CancellationToken cancellationToken);
    public static int RequestCount => Volatile.Read(ref _requestCount);
}