using System.Data;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using http_server;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http;

public class Server : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly IRouteHandler _routeHandler;
    private readonly ILog _log;
    private readonly X509Certificate2? _certificate;

    private Task? _loopTask;
    private CancellationTokenSource _cts = new();
    
    

    public Server(string ip, int port)
    {
        try
        {
            var address = IPAddress.Parse(ip);
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

    public Server(string ip, int port, X509Certificate2 certificate): this(ip, port)
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

    public void Start()
    {
        if (_loopTask == null)
        {
            _loopTask = StartAsync(_cts.Token);
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

    private async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener.Start();
        _log.WriteLine("Server started");
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleConnectionAsync(client, cancellationToken); 
            
        }
    }

    private async Task HandleConnectionAsync(TcpClient client, CancellationToken token = default)
    {
        _log.WriteLine("Connection opened");
        using (client); 
        await using var net = client.GetStream();

        Stream wire = net;
        if (_certificate is not null)
        {
            await using var ssl = new SslStream(net, leaveInnerStreamOpen: false);
            await ssl.AuthenticateAsServerAsync(_certificate); // add ct overloads if you like
            wire = ssl;
        }
        
        var reader = PipeReader.Create(wire);
        var writer = PipeWriter.Create(wire);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        await HandleRequestAsync(reader, writer, cts.Token);
        
    }

    private async Task HandleRequestAsync(PipeReader reader, PipeWriter writer, CancellationToken token = default)
    {
        try
        {
            var fullMessage = await reader.ReadAsync();
            _log.WriteLine(Encoding.ASCII.GetString(fullMessage.Buffer));

            var request = await HttpParser.ParseRequest(reader);
            _log.WriteLine(request.ToString());

            var response = new HttpResponse(HttpCodes.BadRequest, null, "Test bad request");
            var responseBytes = response.FormatResponseAsByteArray();
            await writer.WriteAsync(responseBytes);

            _log.WriteLine("Connection closed");
        }
        catch (Exception e)
        {
            _log.WriteLine(e.ToString());
            try
            {
                var errorResponse = new HttpResponse(HttpCodes.BadRequest);

                var bytes = errorResponse.FormatResponseAsByteArray();
                await writer.WriteAsync(bytes);
            }
            catch (Exception inner)
            {
                _log.WriteLine("Failed to send error response: " + inner);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
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