using System.Runtime.CompilerServices;
using ServeSharp.Core.Path;
using sly.parser;

namespace ServeSharp.Core.Test;

public class PathParserTest
{
    private Parser<RouteToken, Matcher> _parser;

    [SetUp]
    public void Setup()
    {
        // init here to make the warnings show up in the test explorer
        _parser = Parser.New();
    }

    [Test]
    public void TestRootPath()
    {
        var src = @"/";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();

        AssertMatchPath(ret.Result, "/", "", null);
        AssertNonMatchPath(ret.Result, "aaa");
        AssertNonMatchPath(ret.Result, "/aaa");
    }

    [Test]
    public void TestSimpleSubPath()
    {
        var src = @"/path1/path2";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();

        AssertMatchPath(ret.Result, "/path1/path2", "", null);
        AssertNonMatchPath(ret.Result, "/path1");
        AssertNonMatchPath(ret.Result, "/path2");
        AssertNonMatchPath(ret.Result, "/path1/path");
        AssertNonMatchPath(ret.Result, "/path1/path23");
    }

    [Test]
    public void TestSlashTerminatingSubPath()
    {
        var src = @"/path1/path2/";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();

        AssertMatchPath(ret.Result, "/path1/path2/", "", null);
        AssertNonMatchPath(ret.Result, "/path1");
        AssertNonMatchPath(ret.Result, "/path2");
        AssertNonMatchPath(ret.Result, "/path1/path");
        AssertNonMatchPath(ret.Result, "/path1/path2");
        AssertNonMatchPath(ret.Result, "/path1/path23");
        AssertNonMatchPath(ret.Result, "/path1/path2/path3");
        AssertNonMatchPath(ret.Result, "/path1/path2/path3/");
    }

    [Test]
    public void TestComplexMatchingPath1()
    {
        var src = @"/{aaa}/child%aa%bb/114514/{bbb}/fds-{year : /\d{4}/}-{month : /\d{2}/}-{day : /\d{2}/}.html";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

        AssertMatchPath(ret.Result, "/path1/child%aa%bb/114514/path2/fds-2001-02-03.html", "", new Dictionary<string, string>
        {
            {"aaa", "path1"},
            {"bbb", "path2"},
            {"year", "2001"},
            {"month", "02"},
            {"day", "03"},
        });
        AssertNonMatchPath(ret.Result, "/path1");
        AssertNonMatchPath(ret.Result, "/path1child%aa%bb/114514/path2/fds-2001-02-03.html");
        AssertNonMatchPath(ret.Result, "/path1/child%aa%bb/114514/path2/fds-2001-02-0.html");
        AssertNonMatchPath(ret.Result, "/child%aa%bb/114514/path2/fds-2001-02-03.html");
        AssertNonMatchPath(ret.Result, "/path1/child%aa%bb/114514/path2/path3.html");
    }

    [Test]
    public void TestRegexMatchingMultipleSegmentsWithoutEnding()
    {
        var src = @"/path1/{anything: /.*/}";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();

        AssertMatchPath(ret.Result, "/path1/aaa", "", new Dictionary<string, string>
        {
            {"anything", "aaa"},
        });
        AssertMatchPath(ret.Result, "/path1/aaa/bbb", "", new Dictionary<string, string>
        {
            {"anything", "aaa/bbb"},
        });
        AssertMatchPath(ret.Result, "/path1/aaa/bbb/", "", new Dictionary<string, string>
        {
            {"anything", "aaa/bbb/"},
        });
    }

    [Test]
    public void TestRegexMatchingMultipleSegmentsWithEnding()
    {
        var src = @"/path1/{anything: /.*/}/path2/path3";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();

        AssertMatchPath(ret.Result, "/path1/aaa/path2/path3", "", new Dictionary<string, string>
        {
            {"anything", "aaa"},
        });
        AssertMatchPath(ret.Result, "/path1/aaa/bbb/path2/path3/", "", new Dictionary<string, string>
        {
            {"anything", "aaa/bbb"},
        });
        AssertNonMatchPath(ret.Result, "/path1/aaa/path2/");
        AssertNonMatchPath(ret.Result, "/path1/aaa/path2/path");
    }

    private void AssertMatchPath(Matcher matcher, string path, string expectedRemainder, Dictionary<string, string>? expectedBindings)
    {
        var match = matcher.Match(path, out var remainder, out var bindings);

        // test if match
        if (match == false)
        {
            Assert.Fail("route match");
            return;
        }

        // test remainder
        if (!expectedRemainder.Equals(remainder, StringComparison.Ordinal))
        {
            Console.WriteLine($"Expected remainder: {expectedRemainder}");
            Console.WriteLine($"Actual remainder: {remainder}");
            Assert.Fail("remainder mismatch");
        }

        if (!expectedBindings.Equal(bindings))
        {
            Console.WriteLine($"Expected bindings: {expectedBindings}");
            Console.WriteLine($"Actual bindings: {bindings}");
            Assert.Fail("bindings mismatch");
        }

        return;
    }

    private void AssertNonMatchPath(Matcher matcher, string path)
    {
        var match = matcher.Match(path, out var remainder, out var bindings);

        // test if match
        if (match == true)
        {
            Assert.Fail("route match");
            return;
        }
    }
}
