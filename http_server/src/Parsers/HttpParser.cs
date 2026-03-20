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
    private static ReadOnlySpan<byte> http10_suffix => "1.0"u8;
    private static ReadOnlySpan<byte> http11_suffix => "1.1"u8;
    private static ReadOnlySpan<byte> http20_prefix => "PRI *"u8;
    private static ReadOnlySpan<byte> TlsPrefix => [0x16, 0x03];
    private readonly ILog _logger;

    public HttpParser()
    {
        _logger = new Log();
    }
    
    public async Task<bool> LooksLikeTls(PipeReader reader)
    {
        ReadResult readResult = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = readResult.Buffer;

        bool looksLikeTls = LooksLikeTls(buffer);
        reader.AdvanceTo(buffer.Start, buffer.Start);
        return looksLikeTls;
    }

    private static bool LooksLikeTls(ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length < TlsPrefix.Length)
            return false;

        //stackalloc should be faster than TryRead on ReadOnlySequence
        Span<byte> first = stackalloc byte[2];
        buffer.Slice(0, 2).CopyTo(first);
        return first.SequenceEqual(TlsPrefix);
    }
    
    public async Task<HttpVersion> GetHttpVersion(PipeReader reader)
    {
        var header = await reader.ReadAsync();
        var buffer = header.Buffer;
        var httpVersion = DecideVersion(buffer);
        reader.AdvanceTo(buffer.Start, buffer.Start);
        return httpVersion;
    }

    private HttpVersion DecideVersion(ReadOnlySequence<byte> buffer)
    {
        var (line, _) = ReadLineFromBuffer(buffer, Delimiter);
        Span<byte> version = stackalloc byte[5];
        line.Slice(0,5).CopyTo(version);
        if (version.StartsWith(http20_prefix)) return HttpVersion.Http2;
        
        line.Slice(line.Length - 3, 3).CopyTo(version);
        if (version.StartsWith(http11_suffix)) return HttpVersion.Http11;
        else if (version.StartsWith(http10_suffix)) return HttpVersion.Http10;
        return HttpVersion.Http09;
    }

    public async Task<HttpRequest> ParseRequest(PipeReader reader)
    {
        _logger.Debug("Starting HTTP request parsing");

        var http = await ReadRequestLine(reader);
        var pathOnly = http.path.Split("?")[0];
        var queryParams = ParseQueryParams(http.path);
        var headers = await ReadHeadersAsync(reader);

        _logger.Info(
            $"Parsed request {http.method} {pathOnly} {http.version}");

        return HttpRequest.Create(http.method, http.version, pathOnly, headers, queryParams);
    }

    public async Task<(HttpMethod method, string path, HttpVersion version)> ReadRequestLine(PipeReader reader)
    {
        var (line, _) = await ReadLine(reader, Delimiter);
        _logger.Debug($"Raw request line: {line}");

        var http = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (http.Length < 2)
        {
            _logger.Info($"Malformed request line: {line}");
            throw new Exception($"Malformed request line: {line}");
        }

        var method = http[0] switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            _ => throw new Exception($"Invalid HTTP method: {http[0]}")
        };

        var path = http[1];
        var isValidPath = ValidatePath(path);

        HttpVersion httpVersion = HttpVersion.Unknown;

        if (http.Length == 2 && isValidPath)
        {
            httpVersion = HttpVersion.Http09;
        }
        else if (http.Length == 3)
        {
            httpVersion = HttpVersionExtensions.ToHttpVersion(http[2]);
        }

        _logger.Debug(
            $"Parsed request line Method={method}, Path={path}, Version={httpVersion}");

        return (method, path, httpVersion);
    }

    private bool ValidatePath(string path)
    {
        return true;
    }

    private IEnumerable<KeyValuePair<string, string>> ParseQueryParams(string url)
    {
        var queryParams = new Dictionary<string, string>();
        var splitUrl = url.Split('?');

        if (splitUrl.Length < 2)
            return queryParams;

        foreach (var param in splitUrl[1].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = param.Split('=', 2);
            if (parts.Length != 2)
            {
                _logger.Debug($"Skipping malformed query param: {param}");
                continue;
            }

            queryParams[parts[0]] = parts[1];
        }

        return queryParams;
    }

    private async Task<IEnumerable<KeyValuePair<string, string>>> ReadHeadersAsync(PipeReader reader)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        while (true)
        {
            var (header, eof) = await ReadLine(reader, Delimiter);

            if (header == "")
            {
                _logger.Debug($"Finished reading headers. Count={headers.Count}");
                return headers;
            }

            var headerDelimiterPos = header.IndexOf(':');
            if (headerDelimiterPos > 0)
            {
                var headerName = header[..headerDelimiterPos];
                var value = header[(headerDelimiterPos + 1)..].Trim();

                if (headers.ContainsKey(headerName))
                {
                    _logger.Info("Duplicate header encountered: {headerName}");
                }

                headers[headerName] = value;
            }
            else
            {
                _logger.Debug($"Skipping malformed header line: {header}");
            }

            if (eof)
            {
                _logger.Debug("Reached EOF while reading headers");
                return headers;
            }
        }
    }

    private async Task<(string line, bool EOF)> ReadLine(PipeReader reader, byte[] delimiter)
    {
        var res = await reader.ReadAsync();
        var buf = res.Buffer;
        var (line, pos) = ReadLineFromBuffer(buf, delimiter);
        var eof = res.IsCompleted && buf.Length - pos.GetInteger() == 0;
        var lineString = Encoding.ASCII.GetString(line.ToArray());
        reader.AdvanceTo(pos);
        return (lineString, eof);
    }

    private static (ReadOnlySequence<byte>, SequencePosition) ReadLineFromBuffer(
        ReadOnlySequence<byte> buffer,
        ReadOnlySpan<byte> delimiter)
    {
        var sr = new SequenceReader<byte>(buffer);
        if (sr.TryReadTo(out ReadOnlySequence<byte> sequence, delimiter, advancePastDelimiter: true))
        {
            return (sequence, sr.Position);
        }

        return (buffer, buffer.End);
    }
}