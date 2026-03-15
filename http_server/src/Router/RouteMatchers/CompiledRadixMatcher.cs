namespace http_server.Router.RouterResults;

internal class CompiledRadixMatcher<T> : IRouteMatcher<T>
{
    internal CompiledRadixMatcher(RadixRouteMatcher<T>.RadixNode<T> root)
    {
        
    }
    public bool TryMatchRoute(string path, out T route)
    {
        throw new NotImplementedException();
    }
}