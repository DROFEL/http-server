namespace http_server.helpers;

public enum ContentType
{
    None,
    FormUrlEncoded,
    MultipartFormData,
    Json,
    PlainText,
    OctetStream
}

public static class ContentTypeExtensions
{
    public static string ToMime(this ContentType ct) => ct switch
    {
        ContentType.FormUrlEncoded => "application/x-www-form-urlencoded",
        ContentType.MultipartFormData => "multipart/form-data",
        ContentType.Json => "application/json",
        ContentType.PlainText => "text/plain",
        ContentType.OctetStream => "application/octet-stream",
        _ => "none"
    };

    public static ContentType FromMime(string? mime) => mime?.ToLowerInvariant() switch
    {
        "application/x-www-form-urlencoded" => ContentType.FormUrlEncoded,
        "multipart/form-data"               => ContentType.MultipartFormData,
        "application/json"                  => ContentType.Json,
        "text/plain"                        => ContentType.PlainText,
        "application/octet-stream"          => ContentType.OctetStream,
        _ => ContentType.None
    };
}