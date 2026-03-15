namespace http_server.Router.RouterResults;

internal interface IRouteMatcherBuilder<T>
{
    public bool TryAddRoute(string path, T data);
    public IRouteMatcher<T> Compile();
    internal bool TryMatchRoute(string path, out T route);
}