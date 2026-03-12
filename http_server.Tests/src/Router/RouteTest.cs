using System.Reflection;
using http_server.Router;

namespace http_server.Tests.src.Router;

[TestFixture]
[TestOf(typeof(Route))]
public class RouteTest
{
    [Test]
    public void Constructor_Sets_Method_And_Path()
    {
        var attribute = new Route(HttpMethod.Get, "/users");

        Assert.That(attribute.Method, Is.EqualTo(HttpMethod.Get));
        Assert.That(attribute.Path, Is.EqualTo("/users"));
    }

    [Test]
    public void AttributeUsage_Allows_Only_Methods()
    {
        var usage = typeof(Route).GetCustomAttribute<AttributeUsageAttribute>();

        Assert.That(usage, Is.Not.Null);
        Assert.That(usage!.ValidOn, Is.EqualTo(AttributeTargets.Method));
    }

    [Test]
    public void Reflection_Can_Read_Attribute_From_Method()
    {
        var methodInfo = typeof(TestController)
            .GetMethod(nameof(TestController.GetUsers));

        var route = methodInfo!.GetCustomAttribute<Route>();

        Assert.That(route, Is.Not.Null);
        Assert.That(route!.Method, Is.EqualTo(HttpMethod.Get));
        Assert.That(route.Path, Is.EqualTo("/users"));
    }

    private class TestController
    {
        [Route(HttpMethod.Get, "/users")]
        public void GetUsers()
        {
        }
    }
}