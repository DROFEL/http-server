using http_server.Router;
using http_server.Router.RouterResults;
using Prometheus;

namespace http_server.ServerMetrics;

public class PrometheusMetrics
{
    [HTTPRoute(HttpMethod.Get, "/metrics")]
    public static async ValueTask<IHttpResult> PrometheusEndpoint(RouterContext response)
    {
        await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(
            response.Body.AsStream(),
            CancellationToken.None
        );

        return new Ok();
    }
}