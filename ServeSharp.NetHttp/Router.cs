#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ServeSharp.Core.Middleware;
using System.Net.Http;
using System.Text;
using ServeSharp.Core.Path;
using sly.parser;

namespace ServeSharp.NetHttp
{
    public class Router : IPathGroup<Context, Route>
    {
        private readonly List<Route> _routes = new List<Route>();
        private readonly Parser<RouteToken, Matcher> _parser = Parser.New();
        private readonly List<HandleFunc<Context>> _middlewares = new List<HandleFunc<Context>>();

        public bool AutoHead { get; set; } = true;
        public HandleFunc<Context> NotFound { internal get; set; } = DefaultNotFoundHandler;

        public void Use(params HandleFunc<Context>[] middleware) => _middlewares.AddRange(middleware);

        public IPathGroup<Context, Route> Group(string path) => new RouteGroup(this, path);

        internal Route Route(Route route)
        {
            _routes.Add(route);
            return route;
        }

        public Route Route(HttpMethod? method, string path, params HandleFunc<Context> []handlers)
        {
            var pr = _parser.Parse(path);
            pr.ThrowIfError();

            var ret = new Route()
            {
                OriginalRouteDefinition = path,
                Method = method,
                Matcher = pr.Result,
                Middlewares = _middlewares.Concat(handlers).ToArray(),
            };

            return Route(ret);
        }

        public async Middleware Handle(Context context)
        {
#pragma warning disable CA2007
            // StackingAwaiter must be created here so that task continuations are flattened to this level.
            await using var next = new StackingAwaiter();
#pragma warning restore CA2007

            var route = _routes.FirstOrDefault(route => route.Match(context));
            var stack = route == null ? new MiddlewareStack<Context>(_middlewares.Append(NotFound).ToArray()) : new MiddlewareStack<Context>(route.Middlewares);
            await stack.Handle(context, next);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Middleware DefaultNotFoundHandler(Context context, IAwaitable next)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("404 NOT FOUND");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Router]");
            foreach (var r in _routes)
            {
                sb.AppendLine(r.ToString());
            }
            return sb.ToString();
        }
    }
}
