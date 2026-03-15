namespace http_server.Router.RouterResults;

public interface IHttpResult
{
    int StatusCode { get; }
    object? Value { get; }
}