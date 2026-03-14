using http_server.Router;

namespace http_server.Tests.Router;

[TestFixture]
[TestOf(typeof(RouteHandler))]
public class RouteHandlerTest
{
    [Test]
    public void TryRegisterRoute_Then_TryMatchRoute_Returns_Registered_Handler()
    {
        var sut = new RouteHandler();

        Action<RouterContext> handler = _ => { };

        var registered = sut.TryRegisterRoute(HttpMethod.Get, "/users", handler);
        var matched = sut.TryResolve(HttpMethod.Get, "/users", out var foundHandler);

        Assert.That(registered, Is.True);
        Assert.That(matched, Is.True);
        Assert.That(foundHandler, Is.SameAs(handler));
    }

    [Test]
    public void TryMatchRoute_Returns_False_When_Path_Does_Not_Exist()
    {
        var sut = new RouteHandler();

        var matched = sut.TryResolve(HttpMethod.Get, "/missing", out var handler);

        Assert.That(matched, Is.False);
        Assert.That(handler, Is.Null);
    }

    [Test]
    public void TryMatchRoute_Returns_False_When_Method_Does_Not_Exist_For_Path()
    {
        var sut = new RouteHandler();

        Action<RouterContext> getHandler = _ => { };
        sut.TryRegisterRoute(HttpMethod.Get, "/users", getHandler);

        var matched = sut.TryResolve(HttpMethod.Post, "/users", out var foundHandler);

        Assert.That(matched, Is.False);
        Assert.That(foundHandler, Is.Null);
    }

    [Test]
    public void TryRegisterRoute_Returns_False_When_Same_Method_Already_Registered_For_Path()
    {
        var sut = new RouteHandler();

        Action<RouterContext> handler1 = _ => { };
        Action<RouterContext> handler2 = _ => { };

        var first = sut.TryRegisterRoute(HttpMethod.Get, "/users", handler1);
        var second = sut.TryRegisterRoute(HttpMethod.Get, "/users", handler2);

        Assert.That(first, Is.True);
        Assert.That(second, Is.False);
    }

    [Test]
    public void TryRegisterRoute_Allows_Different_Methods_For_Same_Path()
    {
        var sut = new RouteHandler();

        Action<RouterContext> getHandler = _ => { };
        Action<RouterContext> postHandler = _ => { };

        var getRegistered = sut.TryRegisterRoute(HttpMethod.Get, "/users", getHandler);
        var postRegistered = sut.TryRegisterRoute(HttpMethod.Post, "/users", postHandler);

        var getMatched = sut.TryResolve(HttpMethod.Get, "/users", out var foundGet);
        var postMatched = sut.TryResolve(HttpMethod.Post, "/users", out var foundPost);

        Assert.That(getRegistered, Is.True);
        Assert.That(postRegistered, Is.True);
        Assert.That(getMatched, Is.True);
        Assert.That(postMatched, Is.True);
        Assert.That(foundGet, Is.SameAs(getHandler));
        Assert.That(foundPost, Is.SameAs(postHandler));
    }
}