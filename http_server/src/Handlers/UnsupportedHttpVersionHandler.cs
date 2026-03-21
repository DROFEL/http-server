using http_server.Router;

namespace http_server.Handlers;

public class UnsupportedHttpVersionHandler : BaseHttpVersionHandler
{
    public UnsupportedHttpVersionHandler(IRouteHandler routeHandler, HttpHandlerOptions options) : base(routeHandler, options)
    {
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}