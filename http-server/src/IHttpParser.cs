using http_server.helpers;

namespace http;

public interface IHttpParser
{
    public static abstract Task<HttpRequest> ParseRequest(Stream stream);
}