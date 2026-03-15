using System.Reflection;
using http_server.Router.RouterResults;

namespace http_server.Router;

public interface IRouteHandler
{
    public delegate ValueTask<IHttpResult> RouteDelegate(RouterContext context);
    public bool TryRegisterRoute(string method, string path, RouteDelegate handler);
    public bool TryResolve(string method, string path, out RouteDelegate routeMethod);
    public void Build();
}