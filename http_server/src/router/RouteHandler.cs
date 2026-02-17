using System.Reflection;

namespace http_server.router;

public class RouteHandler : IRouteHandler
{
    private Dictionary<string, Dictionary<string, MethodInfo>> _routesByMethod = new();
    public bool TryMatchRoute(HttpMethod method, string path, out MethodInfo routeMethod)
    {
        routeMethod = null;
        return _routesByMethod.TryGetValue(method.ToString(), out var methods) &&
               methods.TryGetValue(path, out routeMethod);
    }

    public bool TryRegisterRoute(string method, string path, Func<int> handler)
    {

        return true;
    }
    
}