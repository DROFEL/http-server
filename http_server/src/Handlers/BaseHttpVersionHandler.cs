using http_server.Router;

namespace http_server.Handlers;

public abstract class BaseHttpVersionHandler: IHttpVersionHandler
{
    protected IRouteHandler RouteHandler { get; }
    protected ConnectionContext Context { get; }

    public BaseHttpVersionHandler(ConnectionContext context, IRouteHandler routeHandler)
    {
        RouteHandler = routeHandler;
        Context = context;
    }

    public abstract Task HandleAsync(CancellationToken ct);

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}