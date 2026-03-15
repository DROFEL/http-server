namespace http_server.Router.RouteMatchers;

internal interface IRouteMatcherBuilder<T>
{
    public bool TryAddRoute(string path, T data);
    public IRouteMatcher<T> Compile();
    internal bool TryMatchRoute(string path, out T route);
}