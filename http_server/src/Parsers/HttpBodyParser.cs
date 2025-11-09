using System.IO.Pipelines;
using System.Text;
using http_server.helpers;

namespace http;

public class HttpBodyParser
{
    public static async Task<string> ParseRequest(HttpRequest request, PipeReader reader)
    {
        if (request.Headers.TryGetValue(HttpHeaderName.ContentType, out var contentType))
        {
            var contentTypeSanitized = contentType.Split(';');
            var type = ContentTypeExtensions.FromMime(contentTypeSanitized[0]);
            switch (type)
            {
                case ContentType.PlainText:
                case ContentType.Json:
                    return await ReadBody(reader, "ASCII");
                    break;
                case ContentType.FormUrlEncoded:
                    return await ReadUrlEncodedBody(reader);
                    break;
                case ContentType.MultipartFormData:
                    return await ReadUrlEncodedBody(reader);
                    break;
                case ContentType.OctetStream:
                    return await ReadUrlEncodedBody(reader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return "";

    }

    private static async Task<string> ReadUrlEncodedBody(PipeReader reader)
    {
        return await ReadBody(reader, "ASCII");
    }
    private static async Task<string> ReadMultipartFormBody(PipeReader reader)
    {
        return await ReadBody(reader, "ASCII");
    }

    private static async Task<string> ReadBody(PipeReader reader, string encoding)
    {
        var res = await reader.ReadAsync();
        return Encoding.ASCII.GetString(res.Buffer);
    }
}