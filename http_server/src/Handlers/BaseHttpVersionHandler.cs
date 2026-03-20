using System.IO.Pipelines;
using http_server.helpers;
using http_server.Parsers;
using http_server.Router;

namespace http_server.Handlers;

public abstract class BaseHttpVersionHandler: IHttpVersionHandler
{
    protected IRouteHandler RouteHandler { get; }
    protected HttpHandlerOptions Options { get; }
    protected PipeWriter Writer { get; }
    protected PipeReader Reader { get; }


    private IHttpParser parser = new HttpParser();
    
    public BaseHttpVersionHandler(IRouteHandler routeHandler, HttpHandlerOptions options)
    {
        RouteHandler = routeHandler;
        Options = options;
        Writer = options.Writer;
        Reader = options.Reader;
    }

    public abstract Task HandleAsync(CancellationToken ct);

    protected async Task<HttpRequest> ParseRequest()
    {
        return await parser.ParseRequest(Reader);
    }

    protected HttpResponse CreateResponse()
    {
        return new HttpResponse(Options.Version, HttpCodes.Ok);
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}