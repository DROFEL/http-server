namespace http_server.Router.RouteMatchers;

internal class CompiledRadixMatcherBuilder<T> : IRouteMatcherBuilder<T>
{
    RadixRouteMatcher<T> _radixMatcher = new ();
    public bool TryAddRoute(string path, T data)
    {
        return _radixMatcher.TryAddRoute(path, data);
    }

    public bool TryMatchRoute(string path, out T route)
    {
        return _radixMatcher.TryMatchRoute(path, out route);
    }
    
    public IRouteMatcher<T> Compile()
    {
        var rootNode = _radixMatcher.Root;
        return new CompiledRadixMatcher<T>(rootNode);
    }
}