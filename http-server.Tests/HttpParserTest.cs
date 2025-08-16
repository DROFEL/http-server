using http;

namespace http_server.Tests;

public class HttpParserTest
{
    [Test]
    public async Task Test1()
    {
        var rawRequest =
            "GET /test HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n";
        byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(rawRequest);

        // Wrap in a MemoryStream
        Stream stream = new MemoryStream(byteArray);
        var request = await HttpParser.ParseRequest(stream);
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.Path, Is.EqualTo("/test"));
            Assert.That(request.Headers["Host"], Is.EqualTo("localhost"));
            Assert.That(request.Headers["User-Agent"], Is.EqualTo("TestClient"));
            Assert.That(request.Body, Is.Null);
        });

    }
}