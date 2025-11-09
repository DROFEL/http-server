using System.IO.Pipelines;
using http_server.helpers;

namespace http;

public interface IHttpMiddleware
{
    public Task<HttpRequest> Next(HttpRequest request);
}