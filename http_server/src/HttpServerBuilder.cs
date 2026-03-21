using System.Net;
using System.Security.Cryptography.X509Certificates;
using http_server.Router;
using Microsoft.Extensions.Logging;

namespace http_server;

public sealed class HttpServerBuilder
{
    private readonly List<HttpListenerDefinition> _listenerDefinitions = new();
    private IRouteHandler _routeHandler = new RouteHandler();
    private ILogger? _log;

    public HttpServerBuilder UseRouter(IRouteHandler routeHandler)
    {
        _routeHandler = routeHandler;
        return this;
    }

    public HttpServerBuilder UseLog(ILogger log)
    {
        _log = log;
        return this;
    }

    public HttpServerBuilder AddListener(IPAddress ipAddress, int port)
    {
        _listenerDefinitions.Add(new HttpListenerDefinition(ipAddress, port));
        return this;
    }

    public HttpServerBuilder AddListener(IPAddress ipAddress, int port, X509Certificate2 certificate)
    {
        _listenerDefinitions.Add(new HttpListenerDefinition(ipAddress, port, certificate));
        return this;
    }

    public HttpServer Build()
    {
        if (_listenerDefinitions.Count == 0)
            throw new InvalidOperationException("At least one listener must be configured.");

        var listeners = _listenerDefinitions.Select(def =>
        {
            var options = new HttpConnectionListenerOptions(def.Certificate);
            return new HttpConnectionListener(
                def.IpAddress,
                def.Port,
                _routeHandler,
                options);
        });

        return new HttpServer(_routeHandler, listeners, _log);
    }
}