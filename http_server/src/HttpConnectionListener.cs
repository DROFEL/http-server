using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using http_server.Handlers;
using http_server.helpers;
using http_server.Parsers;
using http_server.Router;

namespace http_server;

public class HttpConnectionListener : IConnectionHandler
{
    private readonly ILog _log;
    private readonly IHttpParser _parser;
    private readonly IRouteHandler _routeHandler;
    private readonly ITcpConnectionAccepter _accepter;
    private readonly ConcurrentDictionary<long, Task> _activeConnections;
    private long _nextConnectionId = 0;
    private readonly X509Certificate2? _certificate;
    private CancellationTokenSource _cts;

    public HttpConnectionListener(ITcpConnectionAccepter accepter, IRouteHandler routeHandler, X509Certificate2? certificate)
    {
        this._activeConnections = new();
        this._accepter = accepter;
        this._parser = new HttpParser();
        this._routeHandler = routeHandler;
        this._certificate = certificate;
        this._log = new Log();
        this._cts = new CancellationTokenSource();
    }
    
    public async Task StartAsync(CancellationToken ct)
    {
        this._cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _accepter.Start();

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                var stream = await _accepter.AcceptStreamAsync();
                var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
                var connectionId = Interlocked.Increment(ref _nextConnectionId);

                _activeConnections[connectionId] = Task.Run(async () =>
                {
                    try
                    {
                        await HandleConnectionAsync(stream, connectionCts.Token);
                    }
                    finally
                    {
                        _activeConnections.TryRemove(connectionId, out _);
                        connectionCts.Dispose();
                    }
                });
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async Task StopAsync()
    {
        await _cts.CancelAsync();
        _accepter.Stop();
        //Just wait until all http handlers respond to cancellation
        await Task.WhenAll(_activeConnections.Values);
        _cts.Dispose();
    }
    
    public async Task HandleConnectionAsync(Stream wire, CancellationToken token = default)
    {
        HttpServerMetrics.ActiveConnections.Inc();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _log.Debug("Connection opened");

        await using (wire)
        {
            if (_certificate is not null)
            {
                var ssl = new SslStream(wire, leaveInnerStreamOpen: false);
                await ssl.AuthenticateAsServerAsync(_certificate);
                wire = ssl;
            }

            var reader = PipeReader.Create(wire);
            var writer = PipeWriter.Create(wire);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            
            var version = HttpVersion.Http10;
            try
            {
                var httpRequest = await _parser.ParseRequest(reader);
                version = httpRequest.HttpVersion;
                var httpResponse = new HttpResponse(version, HttpCodes.Ok);

                var context = new ConnectionContext(reader, writer, httpRequest, httpResponse, cts.Token);
                _log.Debug($"Request on version {httpRequest.HttpVersion.ToString()} to {httpRequest.Path} with {httpRequest.Method}.");
                var handler = HttpHandlerFactory.Create(httpRequest.HttpVersion, context, _routeHandler);

                _log.Debug($"Request on version {httpRequest.HttpVersion} to {httpRequest.Path} with {httpRequest.Method}.");
                HttpServerMetrics.RequestsTotal.Inc();

                await using (handler)
                {
                    await handler.HandleAsync(cts.Token);
                }
                
            }
            catch (Exception e)
            {
                HttpServerMetrics.RequestsFailed.Inc();
                _log.Info($"Request failed: {e.Message}");
                var userErrorHttpResponse = new HttpResponse(version, 500);
                userErrorHttpResponse.WriteResponseLineAndHeaders(writer);
                await writer.FlushAsync(_cts.Token);
            }
            finally
            {
                stopwatch.Stop();
                HttpServerMetrics.RequestDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                HttpServerMetrics.ActiveConnections.Dec();
            }
        }
    }
    public async ValueTask DisposeAsync()
    {
        await this.StopAsync();
    }
}