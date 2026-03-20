using System.Text;
using http_server.helpers;
using http_server.Router;
using http_server.Router.RouterResults;

namespace http_server.Handlers;

public class Http09Handler :  BaseHttpVersionHandler
{
    public Http09Handler(IRouteHandler routeHandler, HttpHandlerOptions options) : base(routeHandler, options) {}
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var httpRequest = await ParseRequest();
        var httpResponse = CreateResponse();
        if (!RouteHandler.TryResolve(httpRequest.Method.ToString(), httpRequest.Path, out var handler))
        {
            return;
        }
        var routerContext = new RouterContext(httpRequest, httpResponse, Writer);
        var result = await handler.Invoke(routerContext);
        httpResponse.WriteResponseLineAndHeaders(Writer);
        var content = Serializer.SerializeAndWrite(null, Writer, ct);
    }
}