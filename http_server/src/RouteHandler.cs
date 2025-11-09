using System.Reflection;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http;

public class RouteHandler : IRouteHandler
{
    private Dictionary<string, MethodInfo> routes = new Dictionary<string, MethodInfo>();
    public RouteHandler()
    {

    }

    public void RegisterRoute(HttpMethod httpMethod, string path, MethodInfo route)
    {

    }

    public MethodInfo MatchRoute(HttpMethod method, string path)
    {
        return null;
    }

}