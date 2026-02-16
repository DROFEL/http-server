

using System.IO.Pipelines;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http_server.Parsers;

public interface IHttpParser
{
    static abstract Task<HttpRequest> ParseRequest(PipeReader reader);
    static abstract Task<(HttpMethod method, string path, HttpVersion version)> ReadRequestLine(PipeReader reader);
}