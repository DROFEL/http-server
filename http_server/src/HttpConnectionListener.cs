using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
    
    public async Task HandleConnectionAsync(Stream stream, CancellationToken token = default)
    {
        HttpServerMetrics.ActiveConnections.Inc();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _log.Debug("Connection opened");

        var (wire, sslProtocol) = await NegotiateTls(stream, token);
        await using (wire)
        {
            var reader = PipeReader.Create(wire);
            var writer = PipeWriter.Create(wire);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            if (_certificate is null && await _parser.LooksLikeTls(reader))
            {
                _log.Warning($"TSL request on non tsl listener");
                return;
            }
            
            var version = HttpVersion.Http10;
            try
            {
                version = await ResolveVersion(reader, sslProtocol);
                _log.Debug($"Request on version {version}.");
                var handler = HttpHandlerFactory.Create(version, _routeHandler);
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

    private async Task<HttpVersion> ResolveVersion(PipeReader reader, SslApplicationProtocol? sslProtocol)
    {
        if (sslProtocol.HasValue)
        {
            var p = sslProtocol.Value;
            if (p == SslApplicationProtocol.Http11) return HttpVersion.Http11;
            if (p == SslApplicationProtocol.Http2)  return HttpVersion.Http2;
            if (p == SslApplicationProtocol.Http3)  return HttpVersion.Http3;

            throw new NotSupportedException($"Unsupported ALPN protocol: {p}");
                
        }
        return await _parser.GetHttpVersion(reader);
    }

    private async Task<(Stream, SslApplicationProtocol?)> NegotiateTls(Stream wire, CancellationToken ct)
    {
        if (_certificate is not null)
        {
            var options = new SslServerAuthenticationOptions
            {
                ServerCertificate = _certificate,
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                                      | System.Security.Authentication.SslProtocols.Tls13,
                ApplicationProtocols = new List<SslApplicationProtocol>
                {
                    SslApplicationProtocol.Http2,
                    SslApplicationProtocol.Http11
                }
            };
            try
            {
                var ssl = new SslStream(wire, leaveInnerStreamOpen: false);
                await ssl.AuthenticateAsServerAsync(options, ct);
                return (ssl, ssl.NegotiatedApplicationProtocol);
            }
            catch (Exception e)
            {
                _log.Error($"Failed to authenticate TLS: {e.Message}");
                throw;
            }
        }

        return (wire, null);

    }
    public async ValueTask DisposeAsync()
    {
        await this.StopAsync();
    }
}