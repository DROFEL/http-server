using System.Reflection;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http_server;

public interface IRouteHandler
{
    public bool TryMatchRoute(HttpMethod method, string path, out MethodInfo routeMethod);

}