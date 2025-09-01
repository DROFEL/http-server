namespace http_server.Tests;

[TestFixture]
[NonParallelizable]
public class HttpServer_IntegrationTests
{
    HttpServerFixture fixture;
    [OneTimeSetUp]
    public void Setup()
    {
        fixture = new HttpServerFixture();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        fixture.
    }
}