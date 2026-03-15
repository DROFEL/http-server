using http_server.Router;
using http_server.Router.RouterResults;

namespace http_server.Tests.Router;

[TestFixture]
[TestOf(typeof(RouteHandler))]
public class RouteHandlerTest
{
    [Test]
    public void TryRegisterRoute_Then_TryMatchRoute_Returns_Registered_Handler()
    {
        var sut = new RouteHandler();

        IRouteHandler.RouteDelegate handler = _ => { return new ValueTask<IHttpResult>(new Ok());};

        var registered = sut.TryRegisterRoute("GET", "/users", handler);
        var matched = sut.TryResolve("GET", "/users", out var foundHandler);

        Assert.That(registered, Is.True);
        Assert.That(matched, Is.True);
        Assert.That(foundHandler, Is.SameAs(handler));
    }

    [Test]
    public void TryMatchRoute_Returns_False_When_Path_Does_Not_Exist()
    {
        var sut = new RouteHandler();

        var matched = sut.TryResolve("GET", "/missing", out var handler);

        Assert.That(matched, Is.False);
        Assert.That(handler, Is.Null);
    }

    [Test]
    public void TryMatchRoute_Returns_False_When_Method_Does_Not_Exist_For_Path()
    {
        var sut = new RouteHandler();
        IRouteHandler.RouteDelegate getHandler = _ => { return new ValueTask<IHttpResult>(new Ok());};
        sut.TryRegisterRoute("GET", "/users", getHandler);

        var matched = sut.TryResolve("POST", "/users", out var foundHandler);

        Assert.That(matched, Is.False);
        Assert.That(foundHandler, Is.Null);
    }

    [Test]
    public void TryRegisterRoute_Returns_False_When_Same_Method_Already_Registered_For_Path()
    {
        var sut = new RouteHandler();
        IRouteHandler.RouteDelegate handler1 = _ => { return new ValueTask<IHttpResult>(new Ok());};
        IRouteHandler.RouteDelegate handler2 = _ => { return new ValueTask<IHttpResult>(new Ok());};

        var first = sut.TryRegisterRoute("GET", "/users", handler1);
        var second = sut.TryRegisterRoute("GET", "/users", handler2);

        Assert.That(first, Is.True);
        Assert.That(second, Is.False);
    }

    [Test]
    public void TryRegisterRoute_Allows_Different_Methods_For_Same_Path()
    {
        var sut = new RouteHandler();
        IRouteHandler.RouteDelegate getHandler = _ => { return new ValueTask<IHttpResult>(new Ok());};
        IRouteHandler.RouteDelegate postHandler = _ => { return new ValueTask<IHttpResult>(new Ok());};

        var getRegistered = sut.TryRegisterRoute("GET", "/users", getHandler);
        var postRegistered = sut.TryRegisterRoute(HttpMethod.Post, "/users", postHandler);

        var getMatched = sut.TryResolve("GET", "/users", out var foundGet);
        var postMatched = sut.TryResolve("POST", "/users", out var foundPost);

        Assert.That(getRegistered, Is.True);
        Assert.That(postRegistered, Is.True);
        Assert.That(getMatched, Is.True);
        Assert.That(postMatched, Is.True);
        Assert.That(foundGet, Is.SameAs(getHandler));
        Assert.That(foundPost, Is.SameAs(postHandler));
    }
}