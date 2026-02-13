using System.IO.Pipelines;
using http_server.helpers;

namespace http;

public record ConnectionContext(
    PipeReader TransportIn,
    PipeWriter TransportOut,
    HttpRequest HttpRequest,
    CancellationToken CancellationToken
    );