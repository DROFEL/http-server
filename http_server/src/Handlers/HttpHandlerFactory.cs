using http_server.helpers;
using http_server.Router;

namespace http_server.Handlers;

public static class HttpHandlerFactory
{
    public static IHttpVersionHandler Create(HttpVersion v, IRouteHandler router, HttpHandlerOptions options) => v switch
    {
        HttpVersion.Http09 => new Http09Handler(router, options),
        HttpVersion.Http10 => new Http10Handler(router, options),
        HttpVersion.Http11 => new Http11Handler(router, options),
        _ => new UnsupportedHttpVersionHandler(router, options)
    };
}