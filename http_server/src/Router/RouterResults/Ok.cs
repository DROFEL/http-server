namespace http_server.Router.RouterResults;

public class Ok : IHttpResult
{
    public int StatusCode => 200;
    public object? Value { get; }

    public Ok(object? value = null)
    {
        Value = value;
    }
}