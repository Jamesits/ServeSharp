using System.Net;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.NetHttp.Test;

internal sealed class RouterMiddlewareTest
{
    private Router? _router;
    [SetUp]
    public void Setup()
    {
        _router = new Router();
        _router.Use(CorsMiddleware);
        _router.NotFound = NotFoundCrashHandler;

        _router.Any("/root", (context, _) =>
        {
            context.Http.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("root"),
            };
            return Middleware.CompletedTask;
        });
        Console.WriteLine(_router);
    }

    [Test]
    public async Task TestCorsMiddlewareGet1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, "https://example.com/root");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router!.ServeHttp(ctx);
        Assert.That(ctx.Http.Response, Is.Not.Null);
        Assert.That(ctx.Http.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
#pragma warning disable CA2007
        Assert.That(await ctx.Http.Response.Content.ReadAsStringAsync(), Is.EqualTo("root"));
#pragma warning restore CA2007
    }

    // tests an early return middleware
    [Test]
    public async Task TestCorsMiddlewarePreflight1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Options, "https://example.com/root")
        {
            Headers =
            {
                Accept = { }
            }
        };
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router!.ServeHttp(ctx);
        Assert.That(ctx.Http.Response, Is.Not.Null);
        Assert.That(ctx.Http.Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task TestCorsMiddlewarePost1()
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "https://example.com/root")
        {
            Headers =
            {
                Accept = { }
            }
        };
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router!.ServeHttp(ctx);
        Assert.That(ctx.Http.Response, Is.Not.Null);
        Assert.That(ctx.Http.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
#pragma warning disable CA2007
        Assert.That(await ctx.Http.Response.Content.ReadAsStringAsync(), Is.EqualTo("root"));
#pragma warning restore CA2007
    }

    public static Middleware NotFoundCrashHandler(Context context, IAwaitable _)
    {
        context.Http.Response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not Found"),
        };
        return Middleware.CompletedTask;
    }

    public static async Middleware CorsMiddleware(Context context, IAwaitable next)
    {
        ArgumentNullException.ThrowIfNull(context.Http.Request?.RequestUri?.LocalPath);

        if (context.Http.Request.RequestUri.LocalPath.StartsWith("/notrelated1", StringComparison.OrdinalIgnoreCase))
        {
            await next;
            return;
        }

        // handle preflight
        context.Http.Request.Headers.TryGetValues("Origin", out var origin1);
        string origin = origin1?.FirstOrDefault() ?? "http://localhost";

        if (context.Http.Request.Method == HttpMethod.Options)
        {
            context.Http.Response = new HttpResponseMessage(HttpStatusCode.NoContent);
            context.Http.Response.Headers.Add("Access-Control-Allow-Origin", origin);
            context.Http.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Http.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Http.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return;
        }

        await next;

        if (context.Http.Response == null)
        {
            // If the response is null, we still need to set it up for CORS
            context.Http.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(""),
            };
        }

        context.Http.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        context.Http.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Http.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Baggage, Sentry-Trace");
        context.Http.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        return;
    }
}
