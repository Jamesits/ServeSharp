using ServeSharp.Core.Path;
using sly.parser;

namespace ServeSharp.Core.Test;

public class PathParserTest
{
    private Parser<RouteToken, Matcher>? _parser;

    [SetUp]
    public void Setup()
    {
        // init here to make the warnings show up in the test explorer
        _parser = Parser.New();
    }

    [Test]
    public void TestUnparsablePath()
    {
        var src = @"/{";
        var ret = _parser.Parse(src);
        Console.WriteLine(src);
        Assert.Throws<AggregateException>(() =>
        {
            ret.ThrowIfError();
        });
    }

    [Test]
    public void TestRootPath()
    {
        var src = @"/";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

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
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

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
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

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
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

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
        // Regex is greedy by design, so this path would never match
        var src = @"/path1/{anything: /.*/}/path2/path3";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

        AssertNonMatchPath(ret.Result, "/path1/aaa/path2/path3");
        AssertNonMatchPath(ret.Result, "/path1/aaa/bbb/path2/path3/");
        AssertNonMatchPath(ret.Result, "/path1/aaa/path2/");
        AssertNonMatchPath(ret.Result, "/path1/aaa/path2/path");
    }

    [Test]
    public void TestSplatAnything()
    {
        var src = @"/path1/{anything: splat}";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

        AssertNonMatchPath(ret.Result, "/path1");
        AssertMatchPath(ret.Result, "/path1/", "", new Dictionary<string, string>
        {
            {"anything", ""},
        });
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
    public void TestSplatAnythingWithEnding()
    {
        // Splat is greedy by design, so this path would never match
        var src = @"/path1/{anything: splat}/aaa/bbb";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

        AssertNonMatchPath(ret.Result, "/path1");
        AssertNonMatchPath(ret.Result, "/path1/");
        AssertNonMatchPath(ret.Result, "/path1/aaa");
        AssertNonMatchPath(ret.Result, "/path1/aaa/bbb");
        AssertNonMatchPath(ret.Result, "/path1/aaa/bbb/");
        AssertNonMatchPath(ret.Result, "/path1/path2/aaa/bbb");
    }

    [Test]
    public void TestSplitCount()
    {
        var src = @"/path1/{anything: splat(2)}";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

        AssertNonMatchPath(ret.Result, "/path1");
        AssertNonMatchPath(ret.Result, "/path1/");
        AssertNonMatchPath(ret.Result, "/path1/aaa");
        AssertMatchPath(ret.Result, "/path1/aaa/bbb", "", new Dictionary<string, string>
        {
            {"anything", "aaa/bbb"},
        });
        AssertNonMatchPath(ret.Result, "/path1/aaa/bbb/");
        AssertNonMatchPath(ret.Result, "/path1/aaa/bbb/ccc");
    }

    [Test]
    public void TestSplitCountWithEnding()
    {
        var src = @"/path1/{anything: splat(2)}/aaa/bbb";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);

        AssertNonMatchPath(ret.Result, "/path1");
        AssertNonMatchPath(ret.Result, "/path1/");
        AssertNonMatchPath(ret.Result, "/path1/aaa/aaa");
        AssertMatchPath(ret.Result, "/path1/aaa/aaa/aaa/bbb", "", new Dictionary<string, string>
        {
            {"anything", "aaa/aaa"},
        });
        AssertMatchPath(ret.Result, "/path1/aaa/bbb/aaa/bbb", "", new Dictionary<string, string>
        {
            {"anything", "aaa/bbb"},
        });
        AssertNonMatchPath(ret.Result, "/path1/aaa/bbb/");
        AssertNonMatchPath(ret.Result, "/path1/aaa/bbb/ccc");
    }

    private static void AssertMatchPath(Matcher matcher, string path, string expectedRemainder, Dictionary<string, string>? expectedBindings)
    {
        var match = matcher.Match(path, out var remainder, out var bindings);

        // test if match
        if (!match)
        {
            Assert.Fail("route does not match");
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
            Console.WriteLine($"Expected bindings: {expectedBindings.String()}");
            Console.WriteLine($"Actual bindings: {bindings.String()}");
            Assert.Fail("bindings mismatch");
        }

        return;
    }

    private static void AssertNonMatchPath(Matcher matcher, string path)
    {
        var match = matcher.Match(path, out var remainder, out var bindings);

        if (!match) return;
        Assert.Fail("route match");
    }
}
