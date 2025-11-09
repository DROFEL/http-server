using System.Net;
using System.Security.Cryptography.X509Certificates;
using http_server;
using http;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting server");
        var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
        var certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD");
        var cert = TryLoadCertificate(certPath, certPassword);
        var server = new HttpServer(IPAddress.Any, 8080, cert);
        await server.Start();  // Wait for the server to run
        Console.WriteLine("Server stopped");
    }

    private static X509Certificate2? TryLoadCertificate(string? path, string? password)
    {
        try
        {
            if (path == null)
            {
                return null;
            }
            return new X509Certificate2(path, password);
        }
        catch
        {
            return null;
        }
    }
}