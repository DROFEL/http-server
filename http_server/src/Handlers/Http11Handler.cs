using System.Buffers;
using System.IO.Pipelines;
using http_server.helpers;
using http_server.Router;
using http_server.Router.RouterResults;

namespace http_server.Handlers;

public class Http11Handler : BaseHttpVersionHandler
{
    public Http11Handler(IRouteHandler routeHandler, HttpHandlerOptions options) : base(routeHandler, options)
    {
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            Writer.Write(HttpVersionExtensions.Http11Bytes);
            Writer.Write(HttpResponse.CanceledRequestResponsePrefixBytes);
            Writer.Write(HttpResponse.ErrorResponseSuffix);
            await Writer.FlushAsync(ct);
        }

        var httpRequest = await ParseRequest();
        var httpResponse = CreateResponse();
        
        if (!RouteHandler.TryResolve(httpRequest.Method.ToString().ToUpperInvariant(), httpRequest.Path, out var handler))
        {
            Writer.Write(HttpVersionExtensions.Http11Bytes);
            Writer.Write(HttpResponse.NotFoundResponsePrefix);
            Writer.Write(HttpResponse.ErrorResponseSuffix);
            await Writer.FlushAsync(ct);
            return;
        }

        var pipe = new Pipe();
        var routerContext = new RouterContext(httpRequest, httpResponse, pipe.Writer);
        var invokeTask = handler.Invoke(routerContext);
        await pipe.Writer.CompleteAsync();
        
        var result = await invokeTask;
        switch (result)
        {
            case Ok:
            {
                httpResponse._statusCode = (HttpCodes)result.StatusCode;
                break;
            }
        }
        
        var headers = httpResponse._headers;
        headers["Content-Length"] = "100";
        
        httpResponse.WriteResponseLineAndHeaders(Writer);
        await pipe.Reader.CopyToAsync(Writer, ct);
        await Writer.FlushAsync(ct);
    }
}