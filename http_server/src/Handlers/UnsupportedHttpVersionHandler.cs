using http_server.Router;

namespace http_server.Handlers;

public class UnsupportedHttpVersionHandler : BaseHttpVersionHandler
{
    public UnsupportedHttpVersionHandler(IRouteHandler routeHandler) : base(routeHandler)
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