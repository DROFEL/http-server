using Prometheus;

namespace http_server;

public static class HttpServerMetrics
{
    public static readonly Counter RequestsTotal =
        Metrics.CreateCounter("http_server_requests_total", "Total number of HTTP requests");

    public static readonly Counter RequestsFailed =
        Metrics.CreateCounter("http_server_requests_failed_total", "Total failed HTTP requests");

    public static readonly Gauge ActiveConnections =
        Metrics.CreateGauge("http_server_active_connections", "Current active connections");

    public static readonly Histogram RequestDuration =
        Metrics.CreateHistogram("http_server_request_duration_seconds", "Request duration in seconds");
}