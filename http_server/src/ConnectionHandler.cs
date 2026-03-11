using System.IO.Pipelines;
using http_server.Handlers;
using http_server.helpers;
using http_server.Parsers;

namespace http_server;

public class ConnectionHandler
{
    private readonly ILog _log;
    private readonly HttpParser _parser;

    public ConnectionHandler()
    {
        _log = new Log();
    }
    public async Task HandleConnectionAsync(Stream wire, CancellationToken token = default)
    {
        HttpServerMetrics.ActiveConnections.Inc();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _log.Debug("Connection opened");
        await using (wire)
        {
            // if (_certificate is not null)
            // {
            //     await using var ssl = new SslStream(net, leaveInnerStreamOpen: false);
            //     await ssl.AuthenticateAsServerAsync(_certificate);
            //     wire = ssl;
            // }

            var reader = PipeReader.Create(wire);
            var writer = PipeWriter.Create(wire);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            
            try
            {
                var httpRequest = await _parser.ParseRequest(reader);

                var context = new ConnectionContext(reader, writer, httpRequest, cts.Token);
                var handler = HttpHandlerFactory.Create(httpRequest.HttpVersion);
                _log.Debug($"Request on version {httpRequest.HttpVersion.ToString()} to {httpRequest.Path} with {httpRequest.Method}.");
                
                HttpServerMetrics.RequestsTotal.Inc();
                await handler.Accept(context);
            }
            catch (Exception e)
            {
                HttpServerMetrics.RequestsFailed.Inc();
                _log.Info($"Request failed: {e.Message}");
                var userErrorHttpResponse = new HttpResponse(500);
                await wire.WriteAsync(userErrorHttpResponse.FormatResponseAsByteArray(), token);
                return;
            }
            finally
            {
                stopwatch.Stop();
                HttpServerMetrics.RequestDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                HttpServerMetrics.ActiveConnections.Dec();
            }

        }
        
        
    }
}