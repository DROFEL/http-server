using System.Text;
using http_server.helpers;
using http_server.Router;
using http_server.Router.RouterResults;

namespace http_server.Handlers;

public class Http09Handler :  BaseHttpVersionHandler
{
    public Http09Handler(ConnectionContext context, IRouteHandler routeHandler) : base(context, routeHandler) {}
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var httpRequest = Context.HttpRequest;
        if (!RouteHandler.TryResolve(httpRequest.Method.ToString(), httpRequest.Path, out var handler))
        {
            return;
        }
        var routerContext = new RouterContext(httpRequest, Context.HttpResponse, Context.TransportOut);
        var result = await handler.Invoke(routerContext);
        var writer = Context.TransportOut;
        Context.HttpResponse.WriteResponseLineAndHeaders(Context.TransportOut, ct);
        var content = Serializer.SerializeAndWrite(null, Context.TransportOut, ct);
    }
}