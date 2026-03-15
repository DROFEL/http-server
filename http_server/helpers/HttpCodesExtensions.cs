using System.Buffers.Text;

namespace http_server.helpers;

public static class HttpCodesExtensions
{
    public static int WriteStatusCode(this HttpCodes code, Span<byte> destination)
    {
        Utf8Formatter.TryFormat((int)code, destination, out var written);
        return written;
    }
    public static ReadOnlySpan<byte> GetReasonPhraseBytes(this HttpCodes statusCode) =>
        statusCode switch
        {
            HttpCodes.Continue => "Continue"u8,
            HttpCodes.SwitchingProtocols => "Switching Protocols"u8,
            HttpCodes.Processing => "Processing"u8,
            HttpCodes.EarlyHints => "Early Hints"u8,

            HttpCodes.Ok => "OK"u8,
            HttpCodes.Created => "Created"u8,
            HttpCodes.Accepted => "Accepted"u8,
            HttpCodes.NonAuthoritativeInformation => "Non-Authoritative Information"u8,
            HttpCodes.NoContent => "No Content"u8,
            HttpCodes.ResetContent => "Reset Content"u8,
            HttpCodes.PartialContent => "Partial Content"u8,

            HttpCodes.MultiStatus => "Multi-Status"u8,
            HttpCodes.AlreadyReported => "Already Reported"u8,
            HttpCodes.ImUsed => "IM Used"u8,

            HttpCodes.MultipleChoices => "Multiple Choices"u8,
            HttpCodes.MovedPermanently => "Moved Permanently"u8,
            HttpCodes.Found => "Found"u8,
            HttpCodes.SeeOther => "See Other"u8,
            HttpCodes.NotModified => "Not Modified"u8,
            HttpCodes.UseProxy => "Use Proxy"u8,
            HttpCodes.Unused => "Unused"u8,
            HttpCodes.TemporaryRedirect => "Temporary Redirect"u8,
            HttpCodes.PermanentRedirect => "Permanent Redirect"u8,

            HttpCodes.BadRequest => "Bad Request"u8,
            HttpCodes.Unauthorized => "Unauthorized"u8,
            HttpCodes.PaymentRequired => "Payment Required"u8,
            HttpCodes.Forbidden => "Forbidden"u8,
            HttpCodes.NotFound => "Not Found"u8,
            HttpCodes.MethodNotAllowed => "Method Not Allowed"u8,
            HttpCodes.NotAcceptable => "Not Acceptable"u8,
            HttpCodes.ProxyAuthenticationRequired => "Proxy Authentication Required"u8,
            HttpCodes.RequestTimeout => "Request Timeout"u8,
            HttpCodes.Conflict => "Conflict"u8,
            HttpCodes.Gone => "Gone"u8,
            HttpCodes.LengthRequired => "Length Required"u8,
            HttpCodes.PreconditionFailed => "Precondition Failed"u8,
            HttpCodes.ContentTooLarge => "Content Too Large"u8,
            HttpCodes.UriTooLong => "URI Too Long"u8,
            HttpCodes.UnsupportedMediaType => "Unsupported Media Type"u8,
            HttpCodes.RangeNotSatisfiable => "Range Not Satisfiable"u8,
            HttpCodes.ExpectationFailed => "Expectation Failed"u8,
            HttpCodes.ImATeapot => "I'm a teapot"u8,
            HttpCodes.MisdirectedRequest => "Misdirected Request"u8,
            HttpCodes.UnprocessableContent => "Unprocessable Content"u8,
            HttpCodes.Locked => "Locked"u8,
            HttpCodes.FailedDependency => "Failed Dependency"u8,
            HttpCodes.TooEarly => "Too Early"u8,
            HttpCodes.UpgradeRequired => "Upgrade Required"u8,
            HttpCodes.PreconditionRequired => "Precondition Required"u8,
            HttpCodes.TooManyRequests => "Too Many Requests"u8,
            HttpCodes.RequestHeaderFieldsTooLarge => "Request Header Fields Too Large"u8,
            HttpCodes.UnavailableForLegalReasons => "Unavailable For Legal Reasons"u8,

            HttpCodes.InternalServerError => "Internal Server Error"u8,
            HttpCodes.NotImplemented => "Not Implemented"u8,
            HttpCodes.BadGateway => "Bad Gateway"u8,
            HttpCodes.ServiceUnavailable => "Service Unavailable"u8,
            HttpCodes.GatewayTimeout => "Gateway Timeout"u8,
            HttpCodes.HttpVersionNotSupported => "HTTP Version Not Supported"u8,
            HttpCodes.VariantAlsoNegotiates => "Variant Also Negotiates"u8,
            HttpCodes.InsufficientStorage => "Insufficient Storage"u8,
            HttpCodes.LoopDetected => "Loop Detected"u8,
            HttpCodes.NotExtended => "Not Extended"u8,
            HttpCodes.NetworkAuthenticationRequired => "Network Authentication Required"u8,

            _ => "Unknown"u8
        };
}