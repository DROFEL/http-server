using System.Reflection;

namespace http_server.Router;

public interface IRouteHandler
{
    public bool TryRegisterRoute(string method, string path, Action<RouterContext> handler);
    public bool TryRegisterRoute(HttpMethod method, string path, Action<RouterContext> handler);
    public bool TryResolve(HttpMethod method, string path, out Action<RouterContext> routeMethod);
    public void Build();
}