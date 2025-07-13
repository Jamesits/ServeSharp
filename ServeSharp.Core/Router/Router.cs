#nullable enable
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using sly.parser;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;
using System.Linq;

namespace ServeSharp.Core.Router;

public abstract class Router<TContext, TRoute> : IPathGroup<TContext, TRoute>, IServeMux<TContext> where TRoute : IRoute<TContext>
{
    protected List<TRoute> Routes { get; }= [];
    protected Parser<RouteToken, Matcher> Parser { get; } = Path.Parser.New();
    protected List<HandleFunc<TContext>> Middlewares { get; } = [];

    public bool AutoHead { get; set; } = true;
#pragma warning disable CA1044
    public HandleFunc<TContext> NotFound { protected get; set; } = DefaultNotFoundHandler;
#pragma warning restore CA1044

    public void Use(params HandleFunc<TContext>[] handlers) => Middlewares.AddRange(handlers);

    public void Handle(params TRoute[] routes)
    {
        Routes.AddRange(routes);
    }

    public async Middleware.Middleware ServeHttp(TContext context)
    {
        var stack = Routes.FirstOrDefault(route => route.Match(context))?.Stack ?? NotFoundStack;

        // StackingAwaiter must be created here so that task continuations are flattened to this level.
        var next = new StackingAwaiter();
        try
        {
            await stack.Handle(context, next);
        }
        finally
        {
            // https://stackoverflow.com/a/70887681
            await next.DisposeAsync().ConfigureAwait(false);
        }
    }

    private MiddlewareStack<TContext> NotFoundStack => new(Middlewares.Append(NotFound).ToArray());

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Router]");
        foreach (var r in Routes)
        {
            sb.AppendLine(r.ToString());
        }
        return sb.ToString();
    }

    private static Middleware.Middleware DefaultNotFoundHandler(TContext context, IAwaitable next) => throw new NotImplementedException();

    public abstract TRoute Handle(HttpMethod? method, string path, params HandleFunc<TContext>[] handlers);
    public abstract IPathGroup<TContext, TRoute> Group(string path);
}