using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using http_server;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http;

public class Server
{
    private readonly TcpListener _listener;
    private readonly IRouteHandler _routeHandler;
    private readonly ILog _log;

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
            _routeHandler.RegisterRoute(attribute.httpMethod, attribute.path, method);
        }
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine("Server started");
        while (true)
        {
            try
            {

                var client = await this._listener.AcceptTcpClientAsync();
                Console.WriteLine("Connection opended");
                var stream = client.GetStream();
                var request = await HttpParser.ParseRequest(stream);
                Console.Write(request);

                stream.Close();
                Console.WriteLine("Connection closed");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}