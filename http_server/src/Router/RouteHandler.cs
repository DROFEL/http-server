using System.Reflection;
using System.Runtime.CompilerServices;
using http_server.helpers;

namespace http_server.Router;

public class RouteHandler : IRouteHandler
{
    private Dictionary<string, Dictionary<string, MethodInfo>> _routesByMethod = new();
    private ILog _log = new Log();

    public RouteHandler()
    {
        FindAndRegisterMethodsWithAttribute();
    }
    
    public bool TryMatchRoute(HttpMethod method, string path, out MethodInfo routeMethod)
    {
        routeMethod = null;
        return _routesByMethod.TryGetValue(method.ToString(), out var methods) &&
               methods.TryGetValue(path, out routeMethod);
    }

    public bool TryRegisterRoute(string method, string path, Action<RouterContext> handler)
    {

        return true;
    }
    
    public bool TryRegisterRoute(HttpMethod method, string path, Action<RouterContext> handler)
    {

        return true;
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