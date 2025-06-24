using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.NetHttp.Test;

public interface ICustomContext
{
    public string CustomItem1 { get; set; }
}

public static class CustomContextMiddleware
{
    public static ICustomContext CustomContext(this Context context) => context.Get<ICustomContext>();

    public static async Middleware SetContext(Context context, IAwaitable next)
    {
        context.CustomContext().CustomItem1 = "114514";
        await next;
    }

    public static async Middleware AssertContext(Context context, IAwaitable next)
    {
        if (context.CustomContext().CustomItem1 != "114514")
        {
            Assert.Fail("custom content does not match");
        }

        await next;
    }
}

public class MiddlewareTest
{
    private Router _router;
    [SetUp]
    public void Setup()
    {
        _router = new Router();

        _router.Use(CustomContextMiddleware.SetContext);
        _router.Use(CustomContextMiddleware.AssertContext);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        _router.Get("/", async (context, _) => Console.WriteLine("Get root"));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        Console.WriteLine(_router);
    }

    [Test]
    public async Task Test1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router.Handle(ctx);
    }
}
