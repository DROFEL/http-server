using System.Net;
using System.Security.Cryptography.X509Certificates;
using http_server.helpers;
using HttpVersion = http_server.helpers.HttpVersion;

namespace http;

public record HttpServerConfiguration(
    IPAddress Ip,
    X509Certificate2? Ca,
    X509Certificate2? ServerCertificate,
    HttpVersionMask EnableHttpVersion = HttpVersionMask.Http10 | HttpVersionMask.Http11,
    int Port = 80,
    int KeepAliveTimeoutSec = 5,
    int KeepAliveMax = 2);