namespace http_server.helpers;

public enum HttpVersion
{
    Unknown = 0,
    Http10 = 1 << 0,
    Http11 = 1 << 1,
    Http2 = 1 << 2,
    Http3 = 1 << 3
}
[Flags]
public enum HttpVersionMask
{
    Http10 = 1 << 0,
    Http11 = 1 << 1,
    Http2 = 1 << 2,
    Http3 = 1 << 3
}

public static class HttpVersionMaskExtensions
{
    public static bool IsIn(HttpVersionMask mask, HttpVersion version) =>  ((HttpVersionMask)version & mask) != 0;
}

public static class HttpVersionExtensions
{
    public static Version ToVersion(this HttpVersion v) => v switch
    {
        HttpVersion.Http10 => System.Net.HttpVersion.Version10,
        HttpVersion.Http11 => System.Net.HttpVersion.Version11,
        HttpVersion.Http2  => System.Net.HttpVersion.Version20,
        HttpVersion.Http3  => System.Net.HttpVersion.Version30,
        _                => new Version(0, 0)
    };

    public static HttpVersion ToHttpVersion(string version) => version switch
    {
        "HTTP/1.0" => HttpVersion.Http10,
        "HTTP/1.1" => HttpVersion.Http11,
        "HTTP/2.0" => HttpVersion.Http2,
        "HTTP/3.0" => HttpVersion.Http3,
        _ => HttpVersion.Unknown
    };

    public static string FromHttpVersion(this HttpVersion httpVersion) => httpVersion switch
    {
        HttpVersion.Http10 => "HTTP/1.0",
        HttpVersion.Http11 => "HTTP/1.1",
        HttpVersion.Http2 => "HTTP/2.0",
        HttpVersion.Http3 => "HTTP/3.0",
        _ => string.Empty
    };
}