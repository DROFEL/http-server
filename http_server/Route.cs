namespace http_server;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class Route: Attribute
{
    
    public string Path { get; }
    public HttpMethod HttpMethod { get; }

    public Route(HttpMethod httpMethod, string path)
    {
        this.Path = path;
        this.HttpMethod = httpMethod;
    }

    private void LoadRoutes()
    {
        
    }
}