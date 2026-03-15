using http_server.Router;

namespace http_server.Handlers;

public class UnsupportedHttpVersionHandler : BaseHttpVersionHandler
{
    public UnsupportedHttpVersionHandler(ConnectionContext context, IRouteHandler routeHandler) : base(context, routeHandler)
    {
    }
    public Task Accept(ConnectionContext connectionContext)
    {
        return Task.CompletedTask;
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}