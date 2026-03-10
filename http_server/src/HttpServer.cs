using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using http_server.helpers;
using http_server.Handlers;
using http_server.ServerMetrics;
using http_server.Parsers;
using http_server.Router;
using Prometheus;

namespace http_server;

public class HttpServer : IAsyncDisposable
{
    public bool IsReady = false;

    private readonly ITcpConnectionAccepter _accepter;
    private readonly IRouteHandler _routeHandler;
    private readonly ILog _log;
    private readonly ConnectionHandler _connectionHandler;
    private readonly X509Certificate2? _certificate;

    private Task? _loopTask;
    private CancellationTokenSource _cts = new();

    public HttpServer(
        ITcpConnectionAccepter accepter,
        IRouteHandler routeHandler,
        ILog log,
        ConnectionHandler connectionHandler,
        X509Certificate2? certificate = null)
    {
        _accepter = accepter;
        _routeHandler = routeHandler;
        _log = log;
        _connectionHandler = connectionHandler;
        _certificate = certificate;

        _ = typeof(http_server.ServerMetrics.PrometheusMetrics);
    }
    
    public HttpServer(IPAddress ip, int port)
        : this(
            new TcpConnectionAccepter(ip, port),
            new RouteHandler(),
            new Log(),
            new ConnectionHandler())
    {
    }

    public HttpServer(IPAddress ip, int port, X509Certificate2 certificate): this(ip, port)
    {
        this._certificate = certificate;
    }

    public void RegisterController(object controller)
    {
    }
    public async Task StopAsync()
    {
        _cts.Cancel();
        _accepter.Stop();
        if (_loopTask != null)
        {
            try { await _loopTask; }
            catch (OperationCanceledException) { }
        }
    }

    public Task Start()
    {
        var startTask = StartAsync();
        this._loopTask = startTask;
        return startTask;
    }

    private async Task StartAsync()
    {
        _accepter.Start();
        _log.WriteLine("Server started");
        while (!_cts.IsCancellationRequested)
        {
            var stream = await _accepter.AcceptStreamAsync();
            var connectionCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            Task.Run(() => _connectionHandler.HandleConnectionAsync(stream, connectionCts.Token), connectionCts.Token);
        }
    }

    private async Task HandleConnectionAsync(TcpClient client, CancellationToken token = default)
    {
        HttpServerMetrics.ActiveConnections.Inc();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _log.WriteLine("Connection opened");
            using (client)
            {
                await using var net = client.GetStream();

                Stream wire = net;
                if (_certificate is not null)
                {
                    await using var ssl = new SslStream(net, leaveInnerStreamOpen: false);
                    await ssl.AuthenticateAsServerAsync(_certificate);
                    wire = ssl;
                }

                var reader = PipeReader.Create(wire);
                var writer = PipeWriter.Create(wire);
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                var httpRequest = await HttpParser.ParseRequest(reader);

                HttpServerMetrics.RequestsTotal.Inc();

                var context = new ConnectionContext(reader, writer, httpRequest, cts.Token);
                var handler = HttpHandlerFactory.Create(httpRequest.HttpVersion);
                await handler.Accept(context);
            }
        }
        catch (Exception e)
        {
            HttpServerMetrics.RequestsFailed.Inc();
            _log.WriteLine($"Request failed: {e.Message}");
            var userErrorHttpResponse = new HttpResponse(400);
            await client.GetStream().WriteAsync(userErrorHttpResponse.FormatResponseAsByteArray(), token);
        }
        finally
        {
            stopwatch.Stop();
            HttpServerMetrics.RequestDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            HttpServerMetrics.ActiveConnections.Dec();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Dispose();
    }

    private async ValueTask Dispose()
    {
        await _cts.CancelAsync();
        await CastAndDispose(_accepter);
        if (_certificate != null) await CastAndDispose(_certificate);
        if (_loopTask != null) await CastAndDispose(_loopTask);
        await CastAndDispose(_cts);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
}