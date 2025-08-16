using System.Data;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http;

public class HttpParser : IHttpParser
{
    public static async Task<HttpRequest> ParseRequest(Stream stream)
    {
        var reader = new StreamReader(stream);

                var main = await reader.ReadLineAsync();

                if (main == null)
                {
                    throw new ConstraintException("Incorrect http request");
                }

                var http = main.Split(" ");
                var url = http[1].Split("?");
                if (Enum.TryParse<HttpMethod>(http[0], out var method))
                {
                    throw new ConstraintException("Incorrect http request method");
                }
                var path = url[0];
                var httpVersion = http[2].Split("/")[1];

                var queryParams = new Dictionary<string, string>();
                if (url.Length > 1)
                {
                    foreach (var param in url[1].Split("&"))
                    {
                        var parts = param.Split('=');
                        if (parts.Length != 2)
                        {
                            continue;
                        }
                        var key = parts[0];
                        var value = parts[1];
                        queryParams.Add(key, value);
                    }
                }

                var headers = new Dictionary<string, string>();
                string line;
                // Step 1: Read headers
                while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
                {
                    var indexOfSplit = line.IndexOf(":");
                    if (indexOfSplit > 0)
                    {
                        headers.Add(line[0..indexOfSplit], line.Replace(" ", "")[(indexOfSplit + 1)..^0]);
                    }
                }

                char[] body = null;
                if (headers.TryGetValue("Content-Length", out var contentLength))
                {
                    var buffer = new char[int.Parse(contentLength)];
                    await reader.ReadBlockAsync(buffer, 0, buffer.Length);
                    body = buffer;
                }
        return HttpRequest.Create(method, httpVersion, path, headers, queryParams, body);
    }

    static void ParseFormData(string body)
    {

    }

    static void ParseFormUrlEncodedContent(string body)
    {

    }
}