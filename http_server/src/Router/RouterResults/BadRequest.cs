namespace http_server.Router.RouterResults;

public class BadRequest : IHttpResult
{
    public int StatusCode => 200;
    public object? Value { get; }

    public BadRequest(object? value = null)
    {
        Value = value;
    }
}