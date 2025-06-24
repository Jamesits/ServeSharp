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
        public HandleFunc<Context> NotFound { private get; set; } = DefaultNotFoundHandler;

        public void Use(params HandleFunc<Context>[] handlers) => _middlewares.AddRange(handlers);

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

            var ret = new Route(method, pr.Result, _middlewares.Concat(handlers).ToArray())
            {
                OriginalRouteDefinition = path,
            };

            return Route(ret);
        }

        public async Middleware Handle(Context context)
        {
            var stack = _routes.FirstOrDefault(route => route.Match(context))?.Stack ?? NotFoundStack;

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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async Middleware DefaultNotFoundHandler(Context context, IAwaitable next)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Console.WriteLine("404 NOT FOUND");
        }

        private MiddlewareStack<Context> NotFoundStack => new MiddlewareStack<Context>(_middlewares.Append(NotFound).ToArray());

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
