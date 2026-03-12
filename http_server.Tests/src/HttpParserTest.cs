using http_server.Parsers;
using System.IO.Pipelines;
using http_server.helpers;

namespace http_server.Tests;

public class HttpParserTest
{
    private IHttpParser _parser = new HttpParser();
    [TestCase("GET /tes", HttpVersion.Http09)]
    [TestCase("GET /test\r\n", HttpVersion.Http09)]
    [TestCase("GET /test HTTP/1.0\r\n\r\n", HttpVersion.Http10)]
    [TestCase("GET /test HTTP/1.1\r\n\r\n", HttpVersion.Http11)]
    public async Task Parses_Http_Version(string rawRequest, HttpVersion expectedVersion)
    {
        var request = await _parser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.That(request.HttpVersion, Is.EqualTo(expectedVersion));
    }
    
    [TestCase("GET", HttpMethod.Get)]
    [TestCase("POST", HttpMethod.Post)]
    [TestCase("PATCH", HttpMethod.Patch)]
    [TestCase("DELETE", HttpMethod.Delete)]
    [TestCase("HEAD", HttpMethod.Head)]
    [TestCase("OPTIONS", HttpMethod.Options)]
    public async Task Parses_Http_Method(string rawMethod, HttpMethod expectedMethod)
    {
        var rawRequest =
            $"{rawMethod} /test HTTP/1.1\r\n" +
            "\r\n";

        var request = await _parser.ParseRequest(CreateTextPipeReader(rawRequest));

        Assert.That(request.Method, Is.EqualTo(expectedMethod));
    }
    
    [Test]
    public async Task Parse_Http_Headers()
    {
        var rawRequest =
            "GET /test HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n";
        var request = await _parser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers["Host"], Is.EqualTo("localhost"));
            Assert.That(request.Headers["User-Agent"], Is.EqualTo("TestClient"));
            Assert.That(request.Body, Is.Null);
        });

    }
    [Test]
    public async Task HttpHeadersBody()
    {
        var rawRequest =
            "GET /test HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n\r\n" +
            "Some body goes inhere";
        var request = await _parser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers["Host"], Is.EqualTo("localhost"));
            Assert.That(request.Headers["User-Agent"], Is.EqualTo("TestClient"));
            Assert.That(request.Body, Is.Null);
        });
    }
    [Test]
    public async Task HttpIncorrectHeaders()
    {
        var rawRequest =
            "GET /test HTTP/1.1\r\n" +
            "Host localhost\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n";
        var request = await _parser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers["User-Agent"], Is.EqualTo("TestClient"));
            Assert.That(request.Body, Is.Null);
        });
    }
    [Test]
    public async Task HttpEdgeCaseTermination()
    {
        var rawRequest =
            "GET /test HTTP/1.1\r\n" +
            "User-Agent: TestClient";
        var request = await _parser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers["User-Agent"], Is.EqualTo("TestClient"));
            Assert.That(request.Body, Is.Null);
        });
    }
    [Test]
    public async Task HttpQueryParams()
    {
        var rawRequest =
            "GET /test?someParam=1&anotherParam=Param HTTP/1.1\r\n";
        var request = await _parser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers.Count, Is.EqualTo(0));
            Assert.That(request.QueryParameters["someParam"], Is.EqualTo("1"));
            Assert.That(request.QueryParameters["anotherParam"], Is.EqualTo("Param"));
            Assert.That(request.Body, Is.Null);
        });
    }

    private PipeReader CreateTextPipeReader(string text)
    {
        byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(text);
        Stream stream = new MemoryStream(byteArray);
        return PipeReader.Create(stream);
    }
}