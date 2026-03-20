using System.Security.Cryptography.X509Certificates;

namespace http_server;

public record HttpConnectionListenerOptions(
    X509Certificate2? certificate
    );