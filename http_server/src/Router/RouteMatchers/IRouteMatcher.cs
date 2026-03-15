namespace http_server.Router.RouterResults;

public interface IRouteMatcher<T>
{
    public bool TryMatchRoute(string path, out T route);

}