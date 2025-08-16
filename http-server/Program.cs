using http_server;
using http;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting server");
        Server server = new Server("0.0.0.0", 8080);
        await server.Start();  // Wait for the server to run
        Console.WriteLine("Server stopped");
    }
}