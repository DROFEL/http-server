namespace http_server.Router;

[AttributeUsage(AttributeTargets.Method)]
public class HTTPRoute : Attribute
{
    public string Method { get; }
    public string Path { get; }

    public HTTPRoute(HttpMethod method, string path)
    {
        Method = method.ToString().ToUpperInvariant();
        this.Path = path;
    }
    
    public HTTPRoute(string method, string path)
    {
        Method = method;
        this.Path = path;
    }
}