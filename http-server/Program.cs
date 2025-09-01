using System.Security.Cryptography.X509Certificates;
using http_server;
using http;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting server");
        var cert = new X509Certificate2("/Users/drofel/Documents/Projects/certs/server.pfx", "1234");
        // Server server = new Server("0.0.0.0", 8080, cert);
        Server server = new Server("0.0.0.0", 8080, null);
        await server.Start();  // Wait for the server to run
        Console.WriteLine("Server stopped");
    }
}