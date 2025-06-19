using ServeSharp.Core.Path;
using sly.parser;

namespace ServeSharp.Core.Test;

public class RouterParserTest
{
    private Parser<RouteToken, Matcher> _parser;

    [SetUp]
    public void Setup()
    {
        // init here to make the warnings show up in the test explorer
        _parser = Parser.New();
    }

    [Test]
    public void TestAST()
    {
        var src = @"/{aaa}/child%aa%bb/114514/{bbb}/fds-{year : /\d{4}/}-{month : /\d{2}/}-{day : /\d{2}/}.html";
        var ret = _parser.Parse(src);
        ret.ThrowIfError();
        Console.WriteLine(src);
        Console.WriteLine(ret.Result);
    }
}
