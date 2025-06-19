using ServeSharp.Core.Middleware;

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

        Console.WriteLine(_router);
    }

    [Test]
    public async Task TestGet1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, "https://google.com/root");
        using var ctx = new Context()
        {
            Request = msg,
        };
        await _router.Handle(ctx);
    }

    [Test]
    public async Task TestPost1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, "https://google.com/test1/child%aa%bb/114514/test2/fds-2023-01-01.html");
        using var ctx = new Context()
        {
            Request = msg,
        };
        await _router.Handle(ctx);
    }

    public static async Middleware recovery(Context context, DeferrableAwaiter next)
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
