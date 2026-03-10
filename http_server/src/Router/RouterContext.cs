using System.IO.Pipelines;

namespace http_server.Router;

public record RouterContext(
    PipeWriter Body
);