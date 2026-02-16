using System.Net;
using HttpVersion = http_server.helpers.HttpVersion;

namespace http_server.Tests;

[TestFixture]
[NonParallelizable]
public class HttpServerIntegrationTests
{
    private HttpServerFixture _fixture;
    [OneTimeSetUp]
    public void Setup()
    {
        _fixture = new HttpServerFixture(HttpVersion.Http10);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_fixture is not null)
            await _fixture.DisposeAsync();
    }

    [Test]
    public async Task GET_root_returns_200()
    {
        using var req = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/");
        var res = await _fixture.HttpClient.SendAsync(req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}