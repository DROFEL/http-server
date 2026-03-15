namespace http_server.Router.RouterResults;

public class StaticRouteMatcher<T> : IRouteMatcher<T>, IRouteMatcherBuilder<T>
{
    public IRouteMatcher<T> Compile()
    {
        return this;
    }
    
    public bool TryAddRoute(string path, T data)
    {
        throw new NotImplementedException();
    }
    
    public bool TryMatchRoute(string path, out T route)
    {
        throw new NotImplementedException();
    }
}