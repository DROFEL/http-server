using System.Buffers;
using System.IO.Pipelines;
using http_server.helpers;
using http_server.Router;
using http_server.Router.RouterResults;

namespace http_server.Handlers;

public class Http11Handler : BaseHttpVersionHandler
{
    public Http11Handler(ConnectionContext context, IRouteHandler routeHandler) : base(context, routeHandler)
    {
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            this.Context.TransportOut.Write(HttpVersionExtensions.Http11Bytes);
            this.Context.TransportOut.Write(HttpResponse.CanceledRequestResponsePrefixBytes);
            this.Context.TransportOut.Write(HttpResponse.ErrorResponseSuffix);
            await Context.TransportOut.FlushAsync(ct);
        }
        var httpRequest = Context.HttpRequest;
        
        if (!RouteHandler.TryResolve(httpRequest.Method.ToString().ToUpperInvariant(), httpRequest.Path, out var handler))
        {
            Context.TransportOut.Write(HttpVersionExtensions.Http11Bytes);
            Context.TransportOut.Write(HttpResponse.NotFoundResponsePrefix);
            Context.TransportOut.Write(HttpResponse.ErrorResponseSuffix);
            await Context.TransportOut.FlushAsync(ct);
            return;
        }

        var pipe = new Pipe();
        var routerContext = new RouterContext(httpRequest, Context.HttpResponse, pipe.Writer);
        var invokeTask = handler.Invoke(routerContext);
        await pipe.Writer.CompleteAsync();
        
        var writer = Context.TransportOut;
        var result = await invokeTask;
        switch (result)
        {
            case Ok:
            {
                Context.HttpResponse._statusCode = (HttpCodes)result.StatusCode;
                var headers = Context.HttpResponse._headers;
                headers["Content-Length"] = "100";
                break;
            }
        }
        
        Context.HttpResponse.WriteResponseLineAndHeaders(writer);
        await pipe.Reader.CopyToAsync(Context.TransportOut, ct);
        await writer.FlushAsync(ct);
    }
}