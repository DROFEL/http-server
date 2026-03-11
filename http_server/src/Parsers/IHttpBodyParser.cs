using System.IO.Pipelines;
using http_server.helpers;

namespace http_server.Parsers;

public interface IHttpBodyParser
{
    public static abstract Task<string> ParseRequest(HttpRequest request, PipeReader reader);
}