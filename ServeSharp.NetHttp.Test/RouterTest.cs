using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.NetHttp.Test;

public class RouterTest
{
    private Router _router;
    [SetUp]
    public void Setup()
    {
        _router = new Router();
        
        // recovery
        _router.Use(recovery);

        _router.Use(async (context, next) =>
        {
            Console.WriteLine("Middleware 1 enter");
            await next;
            Console.WriteLine("Middleware 1 exit");
        });
        _router.Use(async (context, next) =>
        {
            Console.WriteLine("Middleware 2 enter");
            // throw new NotImplementedException();
            await next;
            // throw new NotImplementedException();
            Console.WriteLine("Middleware 2 exit");
        });

        _router.Get("/root", async (context, _) => Console.WriteLine("Get root"));
        _router.Post(@"/{aaa}/child%aa%bb/114514/{bbb}/fds-{year : /\d{4}/}-{month : /\d{2}/}-{day : /\d{2}/}.html", async (context, _) =>
        {
            Console.WriteLine("Post complex route");
        });
        _router.Group("/group1").Any("/any", async (context, _) => Console.WriteLine("Any"));

        Console.WriteLine(_router);
    }

    [Test]
    public async Task TestGet1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, "https://example.com/root");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router.Handle(ctx);
    }

    [Test]
    public async Task TestPost1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "https://example.com/test1/child%aa%bb/114514/test2/fds-2023-01-01.html");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router.Handle(ctx);
    }

    [Test]
    public async Task TestGroupAny1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Trace, "https://example.com/group1/any");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router.Handle(ctx);
    }

    public static async Middleware recovery(Context context, IAwaitable next)
    {
        Console.WriteLine("Recovery enter");

        try
        {
            await next;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught exception: {ex}");
        }

        Console.WriteLine("Recovery exit");
    }
}
