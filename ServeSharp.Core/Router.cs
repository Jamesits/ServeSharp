#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ServeSharp.Core.Middleware;
using System.Net.Http;
using System.Text;
using sly.parser;
using ServeSharp.Core.Route;

namespace ServeSharp.Core
{
    public class Router
    {
        private readonly List<ServeSharp.Core.Route.Route> _routes = new List<ServeSharp.Core.Route.Route>();
        private readonly Parser<RouteToken, Matcher> _parser = Parser.New();
        private readonly List<HandleFunc> _middlewares = new List<HandleFunc>();

        public bool AutoHead => true;
        public HandleFunc NotFound { get; set; } = DefaultNotFoundHandler;

        public void Use(params HandleFunc[] middleware) => _middlewares.AddRange(middleware);

        public void Route(HttpMethod method, string path, HandleFunc handler)
        {
            var pr = _parser.Parse(path);
            pr.ThrowIfError();

            _routes.Add(new ServeSharp.Core.Route.Route()
            {
                OriginalRouteDefinition = path,
                Method = method,
                Matcher = pr.Result,
                Handler = handler,
            });
        }

        public void Get(string path, HandleFunc handler)
        {
            Route(HttpMethod.Get, path, handler);
            if (AutoHead) Route(HttpMethod.Head, path, handler);
        }
        public void Patch(string path, HandleFunc handler) => Route(HttpMethod.Patch, path, handler);
        public void Post(string path, HandleFunc handler) => Route(HttpMethod.Post, path, handler);
        public void Put(string path, HandleFunc handler) => Route(HttpMethod.Put, path, handler);
        public void Delete(string path, HandleFunc handler) => Route(HttpMethod.Delete, path, handler);
        public void Options(string path, HandleFunc handler) => Route(HttpMethod.Options, path, handler);
        public void Head(string path, HandleFunc handler) => Route(HttpMethod.Head, path, handler);
        public void Trace(string path, HandleFunc handler) => Route(HttpMethod.Trace, path, handler);

        public async ServeSharp.Core.Middleware.Middleware Handle(Context context)
        {
            // DeferrableAwaiter must be created here so that task continuations are flattened to this level.
            var next = new DeferrableAwaiter();
            
            var handleFunc = _routes.Where(route => route.Match(context)).Select(route => route.Handler).FirstOrDefault() ?? NotFound;
            var stack = new HandlerStack(_middlewares.ToArray());
            stack.Add(handleFunc);

            // top half
            try
            {
                await stack.Handle(context, next);
            }
            catch (Exception ex)
            {
                var recover = false;
                if (context.ExceptionHandler != null)
                {
                    recover = context.ExceptionHandler(ex);
                }
                if (!recover) throw;
            }

            // bottom half
            try
            {
                await next.DisposeAsync();
            }
            catch (Exception ex)
            {
                var recover = false;
                if (context.ExceptionHandler != null)
                {
                    recover = context.ExceptionHandler(ex);
                }
                if (!recover) throw;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private static async ServeSharp.Core.Middleware.Middleware DefaultNotFoundHandler(Context context, DeferrableAwaiter next)
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
