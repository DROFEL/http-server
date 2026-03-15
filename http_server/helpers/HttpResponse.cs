using System.Buffers;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;

namespace http_server.helpers;

public class HttpResponse
{
    public static ReadOnlySpan<byte> CanceledRequestResponsePrefixBytes =>
        " 503 Service Unavailable\r\nContent-Length: 0\r\nConnection: close"u8;

    public static ReadOnlySpan<byte> BadRequestResponsePrefix =>
        " 400 Bad Request\r\nContent-Length: 0\r\nConnection: close"u8;

    public static ReadOnlySpan<byte> NotFoundResponsePrefix =>
        " 404 Not Found\r\nContent-Length: 0\r\nConnection: close"u8;

    public static ReadOnlySpan<byte> HttpVersionNotSupportedResponsePrefix =>
        " 505 HTTP Version Not Supported\r\nContent-Length: 0\r\nConnection: close"u8;

    public static ReadOnlySpan<byte> RequestTimeoutResponsePrefix =>
        " 408 Request Timeout\r\nContent-Length: 0\r\nConnection: close"u8;

    public static ReadOnlySpan<byte> HeadersTooLargeResponsePrefix =>
        " 431 Request Header Fields Too Large\r\nContent-Length: 0\r\nConnection: close"u8;

    public static ReadOnlySpan<byte> RequestLineTooLongResponsePrefix =>
        " 414 URI Too Long\r\nContent-Length: 0\r\nConnection: close"u8;

    // Suffix is just the final CRLF to end headers. Date header is written between prefix and suffix.
    public static ReadOnlySpan<byte> ErrorResponseSuffix => "\r\n\r\n"u8;
    public static ReadOnlySpan<byte> Clrf => "\r\n"u8;
    
    
    public HttpCodes _statusCode;
    public readonly Dictionary<string, string> _headers;
    
    private readonly HttpVersion _httpVersion;
    private string? _body;
    

    private HttpResponse()
    {
        _headers = new Dictionary<string, string>();
        _headers.Add(HttpHeaderName.Server, $"{Constants.ServerName}/{Constants.ServerVersion}");
        _headers.Add(HttpHeaderName.Date, DateTime.UtcNow.ToString("r"));
        _headers.Add(HttpHeaderName.ContentType, "text/plain; charset=utf-8");
        _headers.Add(HttpHeaderName.Connection, "Closed");
    }

    public HttpResponse(HttpVersion version, HttpCodes statusCode, IDictionary<string, string>? headers = null,
        object? body = null) : this()
    {
        _httpVersion  = version;
        this._statusCode = statusCode;
        this._headers = headers != null ? _headers.Concat(headers).ToDictionary() : _headers;
        this._body = body != null ? body.ToString() : null;
    }

    public HttpResponse(HttpVersion version, ushort statusCode, IDictionary<string, string>? headers = null,
        object? body = null) : this(version, (HttpCodes)statusCode,
        headers, body)
    {
    }

    public void WriteResponseLineAndHeaders(PipeWriter writer)
    {
        if (_httpVersion != HttpVersion.Http09)
        {
            writer.Write(_httpVersion.ToReadOnlySpan());
            writer.Write(" "u8);
        }

        Span<byte> buffer = stackalloc byte[3];
        var written = _statusCode.WriteStatusCode(buffer);
        var bytes = buffer[..written];
        writer.Write(bytes);

        writer.Write(" "u8);
        writer.Write(_statusCode.GetReasonPhraseBytes());
        writer.Write(Clrf);

        if (_body != null)
        {
            _headers.Add(HttpHeaderName.ContentLength, _body.Length.ToString());
        }

        foreach (var (key, value) in _headers)
        {
            writer.Write(Encoding.UTF8.GetBytes(key));
            writer.Write(": "u8);
            writer.Write(Encoding.UTF8.GetBytes(value));
            writer.Write(Clrf);
        }
        
        writer.Write(Clrf);
    }

    private string GenerateHeadersString()
    {
        var headersCombined = _headers.Select(header => $"{header.Key}: {header.Value}");
        var headersString = string.Join(Constants.CRLF, headersCombined);
        return headersString;
    }
}