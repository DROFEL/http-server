using System.Reflection;
using System.Runtime.CompilerServices;
using http_server.helpers;

namespace http_server.Router;

public class RouteHandler : IRouteHandler
{
    private RadixRouteMatcher<Dictionary<HttpMethod, Action<RouterContext>>> _routesByMethod;
    private ILog _log = new Log();

    public RouteHandler()
    {
        FindAndRegisterMethodsWithAttribute();
    }
    
    public bool TryMatchRoute(HttpMethod method, string path, out Action<RouterContext> routeMethod)
    {
        routeMethod = default;
        return _routesByMethod.TryMatchRoute(method.ToString(), out var methods) &&
               methods.TryGetValue(method, out routeMethod);
    }

    public bool TryRegisterRoute(string method, string path, Action<RouterContext> handler)
    {
        return false;
    }
    
    public bool TryRegisterRoute(HttpMethod method, string path, Action<RouterContext> handler)
    {
        _routesByMethod.TryMatchRoute(path, out var route);
        if (route != null && route.TryGetValue(method, out var methodInfo))
        {
            route.Add(method, handler);

        }
        else if (route == null)
        {
            var dict = new Dictionary<HttpMethod, Action<RouterContext>>();
            dict.Add(method, handler);
            _routesByMethod.TryAddRoute(path, dict);
        }

        return false;
    }
    
    private void FindAndRegisterMethodsWithAttribute()
    {
        var baseDir = AppContext.BaseDirectory;

        var userAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a =>
            {
                try
                {
                    return !a.IsDynamic &&
                           !string.IsNullOrEmpty(a.Location) &&
                           a.Location.StartsWith(baseDir);
                }
                catch
                {
                    return false;
                }
            });

        foreach (var assembly in userAssemblies)
        {
            var methodsWithAttribute = from type in assembly.GetTypes()
                from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                where method.IsDefined(typeof(Route), false)
                select method;
            foreach (var method in methodsWithAttribute)
            {
                var attribute = method.GetCustomAttribute<Route>();
                
                if (attribute != null)
                {
                    _log.Info($"Found Route: {method.Name} at {attribute.Path} with {attribute.Method}");
                    this.TryRegisterRoute(attribute.Method, attribute.Path, method.CreateDelegate<Action<RouterContext>>());
                }
            }
        }
    }
    
}