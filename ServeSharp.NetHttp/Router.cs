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
        private string _groupParentPath = "";

        public bool AutoHead { get; set; } = true;
        public HandleFunc<Context> NotFound { internal get; set; } = DefaultNotFoundHandler;

        public void Use(params HandleFunc<Context>[] middleware) => _middlewares.AddRange(middleware);

        public IPathGroup<Context, Route> Group(string path) => new Router()
        {
            _groupParentPath = path,
        };

        internal Route Route(Route route)
        {
            _routes.Add(route);
            return route;
        }

        public Route Route(HttpMethod method, string path, HandleFunc<Context> handler)
        {
            var pr = _parser.Parse(path);
            pr.ThrowIfError();

            var ret = new Route()
            {
                OriginalRouteDefinition = _groupParentPath + path,
                Method = method,
                Matcher = pr.Result,
                Handler = handler,
            };

            return Route(ret);
        }

        public Route Get(string path, HandleFunc<Context> handler)
        {
            if (AutoHead) Route(HttpMethod.Head, path, handler);
            return Route(HttpMethod.Get, path, handler);
        }
        public Route Patch(string path, HandleFunc<Context> handler) => Route(HttpMethod.Patch, path, handler);
        public Route Post(string path, HandleFunc<Context> handler) => Route(HttpMethod.Post, path, handler);
        public Route Put(string path, HandleFunc<Context> handler) => Route(HttpMethod.Put, path, handler);
        public Route Delete(string path, HandleFunc<Context> handler) => Route(HttpMethod.Delete, path, handler);
        public Route Options(string path, HandleFunc<Context> handler) => Route(HttpMethod.Options, path, handler);
        public Route Head(string path, HandleFunc<Context> handler) => Route(HttpMethod.Head, path, handler);
        public Route Trace(string path, HandleFunc<Context> handler) => Route(HttpMethod.Trace, path, handler);

        public async Middleware Handle(Context context)
        {
#pragma warning disable CA2007
            // StackingAwaiter must be created here so that task continuations are flattened to this level.
            await using var next = new StackingAwaiter();
#pragma warning restore CA2007
            
            var handleFunc = _routes.Where(route => route.Match(context)).Select<Route, HandleFunc<Context>>(route => route.Handler).FirstOrDefault() ?? NotFound;
            var stack = new MiddlewareStack<Context>(_middlewares.ToArray());
            stack.Add(handleFunc);

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
