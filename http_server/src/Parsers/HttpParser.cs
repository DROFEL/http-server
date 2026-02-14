using System.Buffers;
using System.Data;
using System.IO.Pipelines;
using System.Text;
using http_server.helpers;
using HttpMethod = http_server.HttpMethod;

namespace http_server.Parsers;

public class HttpParser : IHttpParser
{
    private static readonly byte[] Delimiter = Encoding.ASCII.GetBytes("\r\n");
    public static async Task<HttpRequest> ParseRequest(PipeReader reader)
    {

        var http = await ReadStatusLine(reader);
        var url = http.path.Split("?");
        var path = url[0];
        var queryParams = ParseQueryParams(http.path);
        var headers = await ReadHeadersAsync(reader);
        
        return HttpRequest.Create(http.method, http.version, path, headers, queryParams);
    }

    public static async Task<(HttpMethod method, string path, HttpVersion version)> ReadStatusLine(PipeReader reader)
    {
        var (line, _) = await ReadLine(reader, Delimiter);
        var http = line.Split(" ");
        if (Enum.TryParse<HttpMethod>(http[0], out var method))
        {
            throw new ConstraintException("Incorrect http request method");
        }
        if (Enum.TryParse<HttpVersion>(http[2], out var version))
        {
            throw new ConstraintException("Incorrect http version");
        }
        return new (method, http[1], version);
    }

    private static IEnumerable<KeyValuePair<string,string>> ParseQueryParams(string url)
    {
        var queryParams = new Dictionary<string, string>();
        var splitUrl = url.Split("?");
        if (splitUrl.Length < 2)
            return queryParams;
        var path = splitUrl[0];
        if (path.Length > 1)
        {
            foreach (var param in path.Split("&"))
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

        return queryParams;
    }
    private static async Task<IEnumerable<KeyValuePair<string,string>>> ReadHeadersAsync(PipeReader reader)
    {
        var headers = new Dictionary<string, string>();
        
        while (true)
        {

            var (header, eof) = await ReadLine(reader, Delimiter); 
            if (header == "")
            {
                return headers;
            }
            var headerDelimiterPos = header.IndexOf(':');
            if (headerDelimiterPos > 0)
            {
                var headerName = header.Substring(0, headerDelimiterPos);
                var value = header.Substring(headerDelimiterPos + 1).Trim();
                if (headers.TryGetValue(headerName, out var headerValue))
                {
                    Console.WriteLine($"{headerName} already exists");
                }
                headers.Add(headerName, value);
            }

            if (eof)
            {
                return headers;
            }
        }
    }

    static async Task<(string line, bool EOF)> ReadLine(PipeReader reader, byte[] delimiter)
    {
        var res = await reader.ReadAsync();
        var buf = res.Buffer;
        var (line, pos) = ReadLineFromBuffer(buf, delimiter);
        reader.AdvanceTo(pos, pos);
        var EOF = res.IsCompleted || buf.Length == 0;
        if (line.Length == 0)
        {
            return (string.Empty, EOF);
        }
        else
        {
            return (Encoding.ASCII.GetString(line), EOF);
        }
    }

    static (ReadOnlySequence<byte>, SequencePosition) ReadLineFromBuffer(
        ReadOnlySequence<byte> buffer,
        ReadOnlySpan<byte> delimiter)
    {
        var sr = new SequenceReader<byte>(buffer);
        if (sr.TryReadTo(out ReadOnlySequence<byte> sequence, delimiter, advancePastDelimiter: true))
        {
            return (sequence, sr.Position);
        }

        return (new ReadOnlySequence<byte>(), sr.Position);
    }
    public static void ParseBody(string body)
    {
        
    }

    static void ParseFormData(string body)
    {

    }

    static void ParseFormUrlEncodedContent(string body)
    {

    }
}