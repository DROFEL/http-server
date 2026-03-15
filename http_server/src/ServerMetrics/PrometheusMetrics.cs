using http_server.Router;
using Prometheus;

namespace http_server.ServerMetrics;

public class PrometheusMetrics
{
    [HTTPRoute(HttpMethod.Get, "/metrics")]
    public static void PrometheusEndpoint(RouterContext response)
    {
        var metricsText = Metrics.DefaultRegistry.CollectAndExportAsTextAsync(response.Body.AsStream());
    }
}