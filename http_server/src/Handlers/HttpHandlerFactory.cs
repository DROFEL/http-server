using http_server.helpers;
using http_server.Router;

namespace http_server.Handlers;

public static class HttpHandlerFactory
{
    public static IHttpVersionHandler Create(HttpVersion v, ConnectionContext context, IRouteHandler router) => v switch
    {
        HttpVersion.Http09 => new Http09Handler(context, router),
        HttpVersion.Http10 => new Http10Handler(context, router),
        HttpVersion.Http11 => new Http11Handler(context, router),
        _ => new UnsupportedHttpVersionHandler(context, router)
    };
}