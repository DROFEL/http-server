using System.Reflection;
using System.Runtime.CompilerServices;
using http_server.helpers;

namespace http_server.Router;

public class RouteHandler : IRouteHandler
{
    private RadixRouteMatcher<Dictionary<HttpMethod, Action<RouterContext>>> _routesByMethod = new();
    private ILog _log = new Log();

    public RouteHandler()
    {
        FindAndRegisterMethodsWithAttribute();
    }
    
    public bool TryMatchRoute(HttpMethod method, string path, out Action<RouterContext> routeMethod)
    {
        routeMethod = default;
        return _routesByMethod.TryMatchRoute(path, out var methods) &&
               methods.TryGetValue(method, out routeMethod);
    }

    public bool TryRegisterRoute(string method, string path, Action<RouterContext> handler)
    {
        return false;
    }
    
    public bool TryRegisterRoute(HttpMethod method, string path, Action<RouterContext> handler)
    {
        _routesByMethod.TryMatchRoute(path, out var route);
        if (route != null && !route.TryGetValue(method, out var methodInfo))
        {
            return route.TryAdd(method, handler);

        }
        else if (route == null)
        {
            var dict = new Dictionary<HttpMethod, Action<RouterContext>>();
            dict.Add(method, handler);
            return _routesByMethod.TryAddRoute(path, dict);
        }

        //If endpoint exists
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

                if (attribute == null)
                    continue;

                if (!IsValidRouteMethod(method))
                {
                    _log.Warning($"Skipping route {method.Name}: incompatible signature");
                    continue;
                }

                _log.Info($"Found Route: {method.Name} at {attribute.Path} with {attribute.Method}");
                var handler = method.CreateDelegate<Action<RouterContext>>();
                TryRegisterRoute(attribute.Method, attribute.Path, handler);
            }
        }
    }
    private static bool IsValidRouteMethod(MethodInfo method)
    {
        if (method.ReturnType != typeof(void))
            return false;

        var parameters = method.GetParameters();
        if (parameters.Length != 1)
            return false;

        if (parameters[0].ParameterType != typeof(RouterContext))
            return false;

        if (!method.IsStatic)
            return false;

        return true;
    }
}