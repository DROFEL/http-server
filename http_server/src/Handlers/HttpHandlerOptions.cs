using System.IO.Pipelines;
using http_server.helpers;

namespace http_server.Handlers;

public record HttpHandlerOptions(
    HttpVersion Version,
    PipeReader Reader,
    PipeWriter Writer
    );