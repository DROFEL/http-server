using System.Net;
using System.Security.Cryptography.X509Certificates;
using http_server.helpers;
using http_server.Router;

namespace http_server;

public class HttpServer : IAsyncDisposable
{
    public bool IsReady = false;
    public IRouteHandler Router
    {
        get { return _routeHandler; }
    }

    
    private readonly ILog _log;
    private readonly HttpConnectionListener _httpConnectionListener;
    private readonly IRouteHandler _routeHandler;
    private readonly X509Certificate2? _certificate;

    private Task? _loopTask;
    private CancellationTokenSource _cts = new();

    public HttpServer(IPAddress ip, int port)
        : this(ip, port, null)
    {
    }

    public HttpServer(IPAddress ip, int port, X509Certificate2? certificate)
    {
        _certificate = certificate;
        _routeHandler = new RouteHandler();
        _log = new Log();
        _httpConnectionListener = new HttpConnectionListener(
            new TcpConnectionAccepter(ip, port),
            _routeHandler,
            _certificate);

        _ = typeof(http_server.ServerMetrics.PrometheusMetrics);
    }
    
    public async Task StopAsync()
    {
        _cts.Cancel();
        await _httpConnectionListener.StopAsync();
        if (_loopTask != null)
        {
            try { await _loopTask; }
            catch (OperationCanceledException) { }
        }
    }

    public Task Start()
    {
        _routeHandler.Build();
        
        var startTask = StartAsync();
        this._loopTask = startTask;
        return startTask;
    }

    private async Task StartAsync()
    {
        await _httpConnectionListener.StartAsync(_cts.Token);
        _log.Info("Server started");
    }

    public async ValueTask DisposeAsync()
    {
        await Dispose();
    }

    private async ValueTask Dispose()
    {
        await _cts.CancelAsync();
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