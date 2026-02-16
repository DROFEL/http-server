using http_server.helpers;

namespace http_server.Handlers;

public static class HttpHandlerFactory
{
    public static IConnectionHandler Create(HttpVersion v) => v switch
    {
        HttpVersion.Http09 => new Http09Handler(),
        HttpVersion.Http10 => new Http10Handler(),
        HttpVersion.Http11 => new Http11Handler(),
        _ => new UnsupportedConnectionHandler()
    };
}