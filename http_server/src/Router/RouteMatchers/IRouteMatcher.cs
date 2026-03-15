namespace http_server.Router.RouteMatchers;

public interface IRouteMatcher<T>
{
    public bool TryMatchRoute(string path, out T route);

}