using http_server.Router;

namespace http_server.Handlers;

public class Http11Handler : BaseHttpVersionHandler
{
    public Http11Handler(ConnectionContext context, IRouteHandler routeHandler) : base(context, routeHandler)
    {
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Task.Delay(1);
    }
}