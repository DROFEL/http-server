using System.Net;
using System.Security.Cryptography.X509Certificates;
using http_server;
using http_server.Router.RouterResults;

namespace Runner;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting server");
        var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
        var certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD");
        var keyPath = Environment.GetEnvironmentVariable("CERT_KEY_PATH");
        var cert = TryLoadCertificate(certPath, certPassword, keyPath);
        var server = new HttpServer(IPAddress.Any, 8080, cert);
        server.Router.TryRegisterRoute("GET", "/heartbeat", async ctx => new Ok("Healthy"));
        await server.Start();  // Wait for the server to run
        Console.WriteLine("Server stopped");
    }

    private static X509Certificate2? TryLoadCertificate(
        string? certPath,
        string? password = null,
        string? keyPath = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(certPath) || !File.Exists(certPath))
                return null;

            var ext = Path.GetExtension(certPath).ToLowerInvariant();

            if (ext is ".pfx" or ".p12")
            {
                return new X509Certificate2(
                    certPath,
                    password,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            }

            if (ext is ".pem" or ".crt" or ".cer")
            {
                X509Certificate2 cert;

                if (!string.IsNullOrWhiteSpace(keyPath))
                {
                    if (!File.Exists(keyPath))
                        return null;

                    cert = string.IsNullOrEmpty(password)
                        ? X509Certificate2.CreateFromPemFile(certPath, keyPath)
                        : X509Certificate2.CreateFromEncryptedPemFile(certPath, password, keyPath);
                }
                else
                {
                    return null;
                }

                return new X509Certificate2(
                    cert.Export(X509ContentType.Pkcs12),
                    (string?)null,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}