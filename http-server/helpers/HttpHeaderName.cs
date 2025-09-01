namespace http_server.helpers;

public readonly record struct HttpHeaderName
{
    public string Value { get; }
    public HttpHeaderName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Empty header name.", nameof(value));
        Value = value.Trim();
    }

    public override string ToString() => Value;
    public bool Equals(HttpHeaderName other) =>
        StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);
    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public static implicit operator string(HttpHeaderName h) => h.Value;
    public static explicit operator HttpHeaderName(string s) => new(s);
    
    public static readonly HttpHeaderName ContentType  = new("Content-Type");
    public static readonly HttpHeaderName Server  = new("Server");
    public static readonly HttpHeaderName Connection  = new("Connection");
    public static readonly HttpHeaderName Date  = new("Date");
    public static readonly HttpHeaderName ContentLength  = new("Content-Length");
    public static readonly HttpHeaderName ContentRange  = new("Content-Range");
    public static readonly HttpHeaderName TransferEncoding  = new("Transfer-Encoding");
    public static readonly HttpHeaderName AcceptEncoding  = new("Accept-Encoding");
    public static readonly HttpHeaderName Accept  = new("Accept");
};