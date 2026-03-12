using http_server.Router;

namespace http_server.Tests.Router;

[TestFixture]
public class TrieRouteMatcherTests
{
    private TrieRouteMatcher<string> _matcher;

    [SetUp]
    public void SetUp()
    {
        _matcher = new TrieRouteMatcher<string>();
    }

    [TestCase("/", "ROOT")]
    [TestCase("/api", "API")]
    [TestCase("/a", "A")]
    [TestCase("a", "A")]
    [TestCase("a/", "A")]
    [TestCase("api", "API")]
    [TestCase("api/", "API")]
    [TestCase("/api/", "API")]
    [TestCase("/a/v1", "V1")]
    [TestCase("/api/v1/", "V1")]
    [TestCase("/api/v1/users", "USERS")]
    [TestCase("/api/v1/users/", "USERS")]
    public void TryMatchRoute(string path, string value)
    {
        var added = _matcher.TryAddRoute(path, value);
        Assert.That(added, Is.True);
        Console.Write(_matcher.ToString());

        var matched = _matcher.TryMatchRoute(path, out var route);

        Assert.That(matched, Is.True);
        Assert.That(route, Is.EqualTo(value));
    }
    
    [TestCaseSource(nameof(AddMultipleCases))]
    public void TryMatchRoute_AddMultiple(List<(string path, string value)> paths)
    {
        foreach (var (path, value) in paths)
        {
            var added = _matcher.TryAddRoute(path, value);
            Assert.That(added, Is.True);
            Console.Write(_matcher.ToString());
        }

        foreach (var (path, value) in paths)
        {
            var matched = _matcher.TryMatchRoute(path, out var route);

            Assert.That(matched, Is.True);
            Assert.That(route, Is.EqualTo(value));
        }
    }
    
    [TestCase("//")]
    [TestCase("")]
    public void TryMatchRoute_InvalidPath(string path)
    {
        var added = _matcher.TryAddRoute("/api", "API");
        Assert.That(added, Is.True);
        Console.Write(_matcher.ToString());

        var matched = _matcher.TryMatchRoute(path, out var route);

        Assert.That(matched, Is.False);
        Assert.That(route, Is.Null);
    }    
    
    [TestCase("/a")]
    [TestCase("/api")]
    [TestCase("api")]
    [TestCase("api/")]
    [TestCase("/api/home")]
    public void TryMatchRoute_WhenRouteDoesntExists(string path)
    {
        var matched = _matcher.TryMatchRoute(path, out var route);

        Assert.That(matched, Is.False);
        Assert.That(route, Is.Null);
    }
    
    private static IEnumerable<TestCaseData> AddMultipleCases()
    {
        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/foo/bar", "foobar"),
            ("/foo/baz", "foobaz"),
        }).SetName("Shared_first_segment").SetDescription("Also test same size different segments");
        
        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/foo/bar", "foobar"),
            ("/foo", "foo"),
            ("/foo/baz", "foobaz"),
        }).SetName("Shared_first_segment_route").SetDescription("Test if common node is a route");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/users/42/details", "user-details"),
            ("/users/42/profile", "user-profile"),
        }).SetName("Shared_prefix_users_42");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/api/v1/users", "users"),
            ("/api/v1/get/users/verified", "verified"),
        }).SetName("Compressed_api_v1_prefix");
        
        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/api/v1/users", "users"),
            ("/api/v1/get/users/verified", "verified"),
            ("/api/v2/users", "users"),
            ("/api/v2/get/users/verified", "verified"),
        }).SetName("Compressed_api_v2_prefix");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/api/v1/users", "users"),
            ("/api/v1/get/users/verified", "verified"),
            ("/api/v1/get/users", "get-users"),
        }).SetName("Compressed_api_v1_with_intermediate_route");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/one", "1"),
            ("/two", "2"),
            ("/three", "3"),
        }).SetName("Sibling_routes");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/foo/", "foo-trailing"),
            ("/foo/bar/", "foobar-trailing"),
        }).SetName("Trailing_slashes");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("//users//42//details//", "details"),
            ("//users//42//profile//", "profile"),
        }).SetName("Repeated_slashes");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/api", "api"),
            ("/api/v1", "apiv1"),
            ("/api/v1/users", "users"),
        }).SetName("Route_is_prefix_of_another_route");

        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("/a", "a"),
            ("/a/b", "ab"),
            ("/a/b/c", "abc"),
            ("/a/b/c/d", "abcd"),
        }).SetName("Deep_nested_routes");
        
        yield return new TestCaseData(new List<(string path, string value)>
        {
            ("api/v1/users/verified", "verified"),
            ("api/v1/products", "products"),
            
        }).SetName("New category").SetDescription("Test changing suffixes");
    }
}