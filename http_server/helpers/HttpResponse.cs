using System.Reflection;
using System.Text;

namespace http_server.helpers;

public class HttpResponse
{
    private readonly string _httpVersion = "1.1"; 
    private readonly HttpCodes _statusCode;
    private string? _body;
    private readonly Dictionary<string, string> _headers;

    private HttpResponse()
    {
        _headers = new Dictionary<string, string>();
        _headers.Add(HttpHeaderName.Server, $"{Constants.ServerName}/{Constants.ServerVersion}");
        _headers.Add(HttpHeaderName.Date, DateTime.UtcNow.ToString("r"));
        _headers.Add(HttpHeaderName.ContentType, "text/plain; charset=utf-8");
        _headers.Add(HttpHeaderName.Connection, "Closed");
    }
    public HttpResponse(HttpCodes statusCode, IDictionary<string, string>? headers = null, object? body = null): this()
    {
        this._statusCode = statusCode;
        this._headers = headers != null ? _headers.Concat(headers).ToDictionary() : _headers;
        this._body = body != null ? body.ToString() : null;
    }

    public HttpResponse(ushort statusCode, IDictionary<string, string>? headers = null, object? body = null) : this((HttpCodes)statusCode,
        headers, body)
    {
        
    }

    public byte[] FormatResponseAsByteArray()
    {
        var responseBuilder = new StringBuilder();
        responseBuilder.Append($"HTTP/{_httpVersion} {(ushort)_statusCode} {_statusCode}");

        if (_body != null)
        {
            _headers.Add(HttpHeaderName.ContentLength, _body.Length.ToString());
        }

        var headersString = GenerateHeadersString();
        if (!string.IsNullOrEmpty(headersString))
            responseBuilder.Append(headersString).Append("\r\n");
        responseBuilder.Append(Constants.CRLF);
        if (_body != null)
        {
            responseBuilder.Append(_body);
        }
        return Encoding.UTF8.GetBytes(responseBuilder.ToString());
    }

    private string GenerateHeadersString()
    {
        var headersCombined = _headers.Select(header => $"{header.Key}: {header.Value}");
        var headersString = string.Join(Constants.CRLF, headersCombined);
        return headersString;
    }
}