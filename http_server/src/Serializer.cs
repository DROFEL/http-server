using System.IO.Pipelines;

namespace http_server;

public class Serializer
{
    public static async Task SerializeAndWrite(object? value, PipeWriter writer, CancellationToken ct)
    {
        if (value != null)
        {
            
        }
    }
}