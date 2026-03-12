using System.Net;
using http_server.helpers;
using http;
using HttpVersion = http_server.helpers.HttpVersion;

namespace http_server.Tests;

public class HttpServerFixture : IAsyncDisposable
{
    HttpServer HttpServer { get; }
    public HttpClient HttpClient { get; }


    public HttpServerFixture(HttpVersion httpVersion)
    {
        var port = GetFreeTcpPort();
        HttpServer = new HttpServer(IPAddress.Loopback, port);
        HttpServer.Start();
        HttpClient = new HttpClient();
        HttpClient.BaseAddress = new Uri($"http://{IPAddress.Loopback.ToString()}:{port}/");
        HttpClient.DefaultRequestVersion = httpVersion.ToVersion();
    }

    private static int GetFreeTcpPort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await HttpServer.DisposeAsync().AsTask();
        }
        catch { /* swallow teardown exceptions to not hide test results */ }
        HttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}