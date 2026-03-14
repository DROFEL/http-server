namespace http_server.Router;

internal interface IRouteMatcherBuilder<T>
{
    public bool TryAddRoute(string path, T data);
    public IRouteMatcher<T> Compile();
    internal bool TryMatchRoute(string path, out T route);
}