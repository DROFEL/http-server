using http_server.Parsers;
using System.IO.Pipelines;

namespace http_server.Tests;

public class HttpParserTest
{
    [Test]
    public async Task HttpWithHeaders()
    {
        var rawRequest =
            "GET /test HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n";
        var request = await HttpParser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Path, Is.EqualTo("/test"));
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
        var request = await HttpParser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Path, Is.EqualTo("/test"));
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
        var request = await HttpParser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Path, Is.EqualTo("/test"));
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
        var request = await HttpParser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Path, Is.EqualTo("/test"));
            Assert.That(request.Headers["User-Agent"], Is.EqualTo("TestClient"));
            Assert.That(request.Body, Is.Null);
        });
    }
    [Test]
    public async Task HttpQueryParams()
    {
        var rawRequest =
            "GET /test?someParam=1&anotherParam=Param HTTP/1.1\r\n";
        var request = await HttpParser.ParseRequest(CreateTextPipeReader(rawRequest));
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Path, Is.EqualTo("/test"));
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