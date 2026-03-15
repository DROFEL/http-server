using System.Buffers;
using http_server.helpers;
using http_server.Router;
using http_server.Router.RouterResults;

namespace http_server.Handlers;

public class Http10Handler :  BaseHttpVersionHandler
{
    public Http10Handler(ConnectionContext context, IRouteHandler routeHandler) : base(context, routeHandler)
    {
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            this.Context.TransportOut.Write(HttpVersionExtensions.Http10Bytes);
            this.Context.TransportOut.Write(HttpResponse.CanceledRequestResponsePrefixBytes);
            this.Context.TransportOut.Write(HttpResponse.ErrorResponseSuffix);
        }
        var httpRequest = Context.HttpRequest;
        
        if (!RouteHandler.TryResolve(httpRequest.Method.ToString(), httpRequest.Path, out var handler))
        {
            Context.TransportOut.Write(HttpVersionExtensions.Http10Bytes);
            Context.TransportOut.Write(HttpResponse.NotFoundResponsePrefix);
            Context.TransportOut.Write(HttpResponse.ErrorResponseSuffix);
            return;
        }

        var routerContext = new RouterContext(httpRequest, Context.HttpResponse, Context.TransportOut);
        var result = await handler.Invoke(routerContext);
        var writer = Context.TransportOut;
        
        switch (result)
        {
            case Ok:
            {
                Context.HttpResponse.WriteResponseLineAndHeaders(writer);
                
                break;
            }
        }
    }
}