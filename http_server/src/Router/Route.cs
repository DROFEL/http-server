namespace http_server.Router;

[AttributeUsage(AttributeTargets.Method)]
public class Route : Attribute
{
    public HttpMethod Method { get; }
    public string Path { get; }

    public Route(HttpMethod method, string path)
    {
        Method = method;
        this.Path = path;
    }
}