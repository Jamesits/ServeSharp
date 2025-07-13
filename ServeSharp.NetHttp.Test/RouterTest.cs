#pragma warning disable CA1303
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.NetHttp.Test;

internal sealed class RouterTest
{
    private Router? _router;
    [SetUp]
    public void Setup()
    {
        _router = new Router();

        // recovery
        _router.Use(Recovery);

        _router.Use(async (_, next) =>
        {
            Console.WriteLine("Middleware 1 enter");
            await next;
            Console.WriteLine("Middleware 1 exit");
        });
        _router.Use(async (_, next) =>
        {
            Console.WriteLine("Middleware 2 enter");
            // throw new NotImplementedException();
            await next;
            // throw new NotImplementedException();
            Console.WriteLine("Middleware 2 exit");
        });

        _router.Get("/root", (_, _) =>
        {
            Console.WriteLine("GetAdapter root");
            return Middleware.CompletedTask;
        });
        _router.Post(@"/{aaa}/child%aa%bb/114514/{bbb}/fds-{year : /\d{4}/}-{month : /\d{2}/}-{day : /\d{2}/}.html", (_, _) =>
        {
            Console.WriteLine("Post complex route");
            return Middleware.CompletedTask;
        });
        _router.Group("/group1").Any("/any", (_, _) =>
        {
            Console.WriteLine("Any");
            return Middleware.CompletedTask;
        });

        Console.WriteLine(_router);
    }

    [Test]
    public async Task TestGet1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, "https://example.com/root");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router!.ServeHttp(ctx);
    }

    [Test]
    public async Task TestPost1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "https://example.com/test1/child%aa%bb/114514/test2/fds-2023-01-01.html");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router!.ServeHttp(ctx);
    }

    [Test]
    public async Task TestGroupAny1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Trace, "https://example.com/group1/any");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router!.ServeHttp(ctx);
    }

    public static async Middleware Recovery(Context context, IAwaitable next)
    {
        Console.WriteLine("Recovery enter");

        try
        {
            await next;
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Console.WriteLine($"Caught exception: {ex}");
        }

        Console.WriteLine("Recovery exit");
    }
}
