using System.Collections.Immutable;
using System.Text.Json;

namespace http_server.helpers;

public sealed record HttpRequest(
    HttpMethod Method,
    string HttpVersion,
    string Path,
    ContentType ContentType,
    ImmutableDictionary<string, string> Headers,
    ImmutableDictionary<string, string> QueryParameters,
    IAsyncEnumerable<ReadOnlyMemory<byte>>? Body
)
{
    public static HttpRequest Create(
        HttpMethod method,
        string httpVersion,
        string path,
        IEnumerable<KeyValuePair<string,string>>? headers = null,
        IEnumerable<KeyValuePair<string,string>>? query = null,
        IAsyncEnumerable<ReadOnlyMemory<byte>>? body = null)
    {
        var headerMap = (headers ?? Array.Empty<KeyValuePair<string,string>>())
            .ToImmutableDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

        var queryMap = (query ?? Array.Empty<KeyValuePair<string,string>>())
            .ToImmutableDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        headerMap.TryGetValue("Content-Type", out var ctHeader);
        var ct = ContentTypeExtensions.FromMime(ctHeader);

        return new HttpRequest(method, httpVersion, path, ct, headerMap, queryMap, body);
    }

    public override string ToString()
    {
        return $"{Method} {HttpVersion} {Path}. Headers: {JsonSerializer.Serialize(Headers)}. Content: {ContentType.ToMime()}";
    }
}