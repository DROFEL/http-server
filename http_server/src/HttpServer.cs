using System.Net;
using System.Security.Cryptography.X509Certificates;
using http_server.helpers;
using http_server.Router;
using Microsoft.Extensions.Logging;

namespace http_server;

public class HttpServer : IAsyncDisposable
{
    public bool IsReady = false;
    public IRouteHandler Router
    {
        get { return _routeHandler; }
    }

    
    private readonly ILogger _log;
    private readonly List<HttpConnectionListener> _httpConnectionListeners;
    private readonly IRouteHandler _routeHandler;

    private Task? _loopTask;
    private CancellationTokenSource _cts = new();

    public HttpServer()
    {
        _routeHandler = new RouteHandler();
        _httpConnectionListeners = new();
        _log = new Logger();

        _ = typeof(http_server.ServerMetrics.PrometheusMetrics);
    }

    public void AddListener(IPAddress address, int port, X509Certificate2? certificate = null)
    {
        var httpConnectionListenerOptions = new HttpConnectionListenerOptions(address, port, certificate);
        var httpConnectionListener = new HttpConnectionListener(
            _routeHandler,
            httpConnectionListenerOptions
        );
        _httpConnectionListeners.Add(httpConnectionListener);
    }
    
    public async Task StopAsync()
    {
        await _cts.CancelAsync();
        foreach (var httpConnectionListener in _httpConnectionListeners)
        {
            await httpConnectionListener.StopAsync();
        }
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
        foreach (var httpConnectionListener in _httpConnectionListeners)
        {
            await httpConnectionListener.StartAsync(_cts.Token);
        }

        _log.Log(LogLevel.Information,"Server started");
    }

    public async ValueTask DisposeAsync()
    {
        await Dispose();
    }

    private async ValueTask Dispose()
    {
        await _cts.CancelAsync();
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