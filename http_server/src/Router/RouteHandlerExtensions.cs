namespace http_server.Router;

public static class RouteHandlerExtensions
{
    public static bool TryRegisterRoute(
        this IRouteHandler routes,
        HttpMethod method,
        string path,
        IRouteHandler.RouteDelegate handler)
    {
        return routes.TryRegisterRoute(method.ToString().ToUpperInvariant(), path, handler);
    }
}