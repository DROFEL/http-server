using System.Reflection;
using http_server.helpers;
using http_server.Router.RouterResults;

namespace http_server.Router;

public class RouteHandler : IRouteHandler
{
    private readonly ILog _log = new Log();

    private IRouteMatcherBuilder<Dictionary<string, IRouteHandler.RouteDelegate>>? _builder;
    private IRouteMatcher<Dictionary<string, IRouteHandler.RouteDelegate>> _matcher;

    private bool _built;

    public RouteHandler(MatcherStrategy strategy = MatcherStrategy.Radix)
    {
        switch (strategy)
        {
            case MatcherStrategy.Radix:
                _builder = new RadixRouteMatcher<Dictionary<string, IRouteHandler.RouteDelegate>>();
                break;
            case MatcherStrategy.CompiledRadix:
                break;
            case MatcherStrategy.Static:
                break;
        }
    }
    public bool TryResolve(string method, string path, out IRouteHandler.RouteDelegate routeMethod)
    {
        routeMethod = default;
        if (!_matcher.TryMatchRoute(path, out var methods)) 
            return false;
        return methods.TryGetValue(method, out routeMethod);
    }

    public void Build()
    {
        if (_built)
            return;

        if (_builder is null)
            throw new InvalidOperationException("No route builder available.");

        _matcher = _builder.Compile();
        _builder = null;
        _built = true;
    }

    public bool TryRegisterRoute(string method, string path, IRouteHandler.RouteDelegate handler)
    {
        _builder.TryMatchRoute(path, out var route);
        if (route != null && !route.TryGetValue(method, out var methodInfo))
        {
            return route.TryAdd(method, handler);

        }
        else if (route == null)
        {
            var dict = new Dictionary<string, IRouteHandler.RouteDelegate>();
            dict.Add(method, handler);
            return _builder.TryAddRoute(path, dict);
        }

        //If endpoint exists
        return false;
    }
    
    private void EnsureNotBuilt()
    {
        if (_built)
            throw new InvalidOperationException("Routes have already been built.");
    }
    
    private void DiscoverRoutesFromAttributes()
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
                where method.IsDefined(typeof(HTTPRoute), false)
                select method;
            foreach (var method in methodsWithAttribute)
            {
                var attribute = method.GetCustomAttribute<HTTPRoute>();

                if (attribute == null)
                    continue;

                if (!IsValidRouteMethod(method))
                {
                    _log.Warning($"Skipping route {method.Name}: incompatible signature");
                    continue;
                }

                _log.Info($"Found Route: {method.Name} at {attribute.Path} with {attribute.Method}");
                var handler = method.CreateDelegate<IRouteHandler.RouteDelegate>();
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