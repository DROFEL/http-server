using http_server.helpers;

namespace http.Handlers;

public static class HttpHandlerFactory
{
    public static IConnectionHandler Create(HttpVersion v) => v switch
    {
        HttpVersion.Http10 => new Http10Handler(),
        HttpVersion.Http11 => new Http11Handler(),
        _ => new UnsupportedConnectionHandler()
    };
}