using http_server.helpers;
using http_server.Router;

namespace http_server.Handlers;

public static class HttpHandlerFactory
{
    public static IHttpVersionHandler Create(HttpVersion v, IRouteHandler router) => v switch
    {
        HttpVersion.Http09 => new Http09Handler(router),
        HttpVersion.Http10 => new Http10Handler(router),
        HttpVersion.Http11 => new Http11Handler(router),
        _ => new UnsupportedHttpVersionHandler(router)
    };
}