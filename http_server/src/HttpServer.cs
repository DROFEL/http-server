using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using http_server;
using http_server.helpers;
using http_server.Handlers;
using http_server.Parsers;

namespace http_server;

public class HttpServer : IAsyncDisposable
{
    public bool IsReady = false;

    private readonly TcpListener _listener;
    private readonly IRouteHandler _routeHandler;
    private readonly ILog _log;
    private readonly X509Certificate2? _certificate;

    private Task? _loopTask;
    private CancellationTokenSource _cts = new();

    public HttpServer(IPAddress ip, int port)
    {
        try
        {
            var address = ip;
            this._listener = new TcpListener(address, port);
            this._routeHandler = new RouteHandler();
            this._log = new Log();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public HttpServer(IPAddress ip, int port, X509Certificate2 certificate): this(ip, port)
    {
        this._certificate = certificate;
    }

    public void RegisterController(object controller)
    {
        var controllerType = controller.GetType();
        var methods = controllerType.GetMethods();
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<Route>();
            if (attribute == null)
            {
                continue;
            }
            _routeHandler.RegisterRoute(attribute.HttpMethod, attribute.Path, method);
        }
    }
    public async Task StopAsync()
    {
        _cts.Cancel();
        _listener.Stop();
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
        _listener.Start();
        _log.WriteLine("Server started");
        while (!_cts.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync();
            var connectionCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            _ = HandleConnectionAsync(client, connectionCts.Token);
            
        }
    }

    private async Task HandleConnectionAsync(TcpClient client, CancellationToken token = default)
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
            
            var context = new ConnectionContext(reader, writer, httpRequest, cts.Token);
            var handler = HttpHandlerFactory.Create(httpRequest.HttpVersion);
            await handler.Accept(context);

        }
        
    }

    public async ValueTask DisposeAsync()
    {
        await Dispose();
    }

    private async ValueTask Dispose()
    {
        await _cts.CancelAsync();
        await CastAndDispose(_listener);
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