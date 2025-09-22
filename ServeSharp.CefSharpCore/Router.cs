using ServeSharp.Core;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Router;
using System.IO;
using System.Linq;
using System.Net.Http;
using ServeSharp.Core.Path;
using CefSharp;
using System.Threading.Tasks;

namespace ServeSharp.CefSharpCore;

public class Router : Router<Context, Route>, IPathGroup<Context, Route>, ISchemeHandlerFactory
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

    private static Task DefaultNotFoundHandler(Context context, IAwaitable _)
    {
        var handler = new ContinuingResourceHandler()
        {
            StatusCode = 404,
            MimeType = "text/plain",
            Stream = new MemoryStream("404 Not Found"u8.ToArray())
        };
        context.Http.ResourceHandler = handler;
        return Task.CompletedTask;
    }

    public async Task<IResourceHandler> ServeHttp(IBrowser browser, IFrame frame, string schemeName, IRequest request)
    {
        using var context = new Context();
        context.Http.Browser = browser;
        context.Http.Frame = frame;
        context.Http.SchemeName = schemeName;
        context.Http.Request = request;

        await ServeHttp(context).ConfigureAwait(false);

        // do not dispose the ResourceHandler with the context
#pragma warning disable CA2000
        var ret = context.Http.ResourceHandler ?? new CancellingResourceHandler();
#pragma warning restore CA2000
        context.Http.ResourceHandler = null;
        return ret;
    }

    #region impl of ISchemeHandlerFactory

    public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request) =>
        ServeHttp(browser, frame, schemeName, request).GetAwaiter().GetResult();

    #endregion
}
