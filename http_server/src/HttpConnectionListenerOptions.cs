using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace http_server;

public record HttpConnectionListenerOptions(
    IPAddress Address,
    int Port,
    X509Certificate2? certificate
    );