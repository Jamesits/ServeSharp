using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServeSharp.Core;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using ServeSharp.Core.Router;

namespace ServeSharp.AspNetCore;

public class Router : Router<Context, Route>, IPathGroup<Context, Route>
{
    public Router()
    {
        NotFound = DefaultNotFoundHandler;
    }

    public override IPathGroup<Context, Route> Group(string path) => new RouteGroup(this, path);

    internal Route Route(Route route)
    {
        Routes.Add(route);
        return route;
    }

    public override Route Handle(HttpMethod? method, string path, params HandleFunc<Context>[] handlers)
    {
        var pr = Parser.Parse(path);
        pr.ThrowIfError();

        var ret = new Route(method, pr.Result, Middlewares.Concat(handlers).ToArray())
        {
            OriginalRouteDefinition = path,
        };

        return Route(ret);
    }

    private static async Task DefaultNotFoundHandler(Context context, IAwaitable _)
    {
        context.Http.HttpContext!.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Http.HttpContext!.Response.ContentType = "text/plain";
        await context.Http.HttpContext.Response.WriteAsync("404 NOT FOUND").ConfigureAwait(false);
    }

    public async Task ServeHttp(HttpContext httpContext)
    {
        using var ctx = new Context();
        ctx.Http.HttpContext = httpContext;

        await ServeHttp(ctx).ConfigureAwait(false);

        // prevent context being disposed
        ctx.Http.HttpContext = null;
    }
}
