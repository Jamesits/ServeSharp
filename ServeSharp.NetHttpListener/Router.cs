using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ServeSharp.Core;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using ServeSharp.Core.Router;

namespace ServeSharp.NetHttpListener;

public class Router : Router<Context, Route>, IPathGroup<Context, Route>
{
    public Router()
    {
        NotFound = DefaultNotFoundHandler;
    }

    public override IPathGroup<Context, Route> Group(string path) => new RouteGroup(this, path);

    public override Route Handle(HttpMethod? method, string path, params HandleFunc<Context>[] handlers)
    {
        var pr = Parser.Parse(path);
        pr.ThrowIfError();

        var ret = new Route(method, pr.Result, Middlewares.Concat(handlers).ToArray())
        {
            OriginalRouteDefinition = path,
        };

        Handle(ret);
        return ret;
    }

    private static Middleware DefaultNotFoundHandler(Context context, IAwaitable next)
    {
        Console.WriteLine("404 NOT FOUND");
        return Middleware.CompletedTask;
    }

    public async Task ServeHttp(HttpListenerContext httpListenerContext)
    {
        using var ctx = new Context();
        ctx.Http.HttpListenerContext = httpListenerContext;
        await ServeHttp(ctx);
    }
}
