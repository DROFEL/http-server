using http;

namespace http_server.Tests;

public class HttpServerFixture : IDisposable
{
    Server Server { get; }
    HttpClient HttpClient { get; }

    public HttpServerFixture()
    {
        Server = new Server("127.0.0.1", 8080);
        Server.Start(); 
        HttpClient = new HttpClient();
        HttpClient.BaseAddress = new Uri("http://localhost:8080/");
    }

    public void Dispose()
    { 
        Server.StopAsync();
        HttpClient.Dispose();
    }
}