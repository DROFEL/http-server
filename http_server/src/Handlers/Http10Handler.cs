using System.Buffers;
using http_server.helpers;
using http_server.Router;
using http_server.Router.RouterResults;

namespace http_server.Handlers;

public class Http10Handler :  BaseHttpVersionHandler
{
    public Http10Handler(IRouteHandler routeHandler, HttpHandlerOptions options) : base(routeHandler, options)
    {
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            Writer.Write(HttpVersionExtensions.Http10Bytes);
            Writer.Write(HttpResponse.CanceledRequestResponsePrefixBytes);
            Writer.Write(HttpResponse.ErrorResponseSuffix);
        }
        var httpRequest = await ParseRequest();
        var httpResponse = CreateResponse();
        
        if (!RouteHandler.TryResolve(httpRequest.Method.ToString(), httpRequest.Path, out var handler))
        {
            Writer.Write(HttpVersionExtensions.Http10Bytes);
            Writer.Write(HttpResponse.NotFoundResponsePrefix);
            Writer.Write(HttpResponse.ErrorResponseSuffix);
            return;
        }

        var routerContext = new RouterContext(httpRequest, httpResponse, Writer);
        var result = await handler.Invoke(routerContext);
        
        switch (result)
        {
            case Ok:
            {
                httpResponse.WriteResponseLineAndHeaders(Writer);
                
                break;
            }
        }
    }
}