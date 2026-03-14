namespace http_server.Router;

public interface IRouteMatcher<T>
{
    public bool TryMatchRoute(string path, out T route);

}