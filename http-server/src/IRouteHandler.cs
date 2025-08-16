using System.Reflection;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http;

public interface IRouteHandler
{
    public void RegisterRoute(HttpMethod httpMethod, string path, MethodInfo route);
    public MethodInfo MatchRoute(HttpMethod method, string path);

}