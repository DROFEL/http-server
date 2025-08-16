namespace http_server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class Route: Attribute
{
    public string path { get; }
    public HttpMethod httpMethod { get; }

    public Route(HttpMethod httpMethod, string path)
    {
        this.path = path;
        this.httpMethod = httpMethod;
    }
}