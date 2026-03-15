using System.IO.Pipelines;
using http_server.helpers;

namespace http_server.Router;

public record RouterContext(
    HttpRequest Request,
    HttpResponse Response,
    PipeWriter Body
);