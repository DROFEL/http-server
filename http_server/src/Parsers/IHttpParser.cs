

using System.IO.Pipelines;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http_server.Parsers;

public interface IHttpParser
{
    abstract Task<HttpRequest> ParseRequest(PipeReader reader);
    abstract Task<(HttpMethod method, string path, HttpVersion version)> ReadRequestLine(PipeReader reader);
    abstract Task<HttpVersion> GetHttpVersion(PipeReader reader);
    public Task<bool> LooksLikeTls(PipeReader reader);
}