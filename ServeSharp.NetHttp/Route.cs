#nullable enable
using System;
using System.Net.Http;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.NetHttp
{
    public class Route
    {
        public string Name { get; set; } = "UNNAMED";
        public string OriginalRouteDefinition { get; internal set; } = "";
        public Matcher Matcher { get; internal set; }
        public HttpMethod? Method { get; internal set; }
        public HandleFunc<Context>[] Middlewares { get; internal set; }

        public override string ToString() => $"{Name} {Method?.ToString() ?? "ANY"} Handler({Middlewares.Length}) {OriginalRouteDefinition}";

        public bool Match(Context context)
        {
            if (context?.Http?.Request == null)
            {
                throw new InvalidOperationException("Request is null");
            }

            // method == null: match any
            if (Method != null)
            {
                // test method
                if (Method != context.Http.Request.Method) return false;
            }
            
            // test path
            var ret = Matcher.Match(context.Http.Request.RequestUri.AbsolutePath, out _, out var bindings);
            context.Http.UrlBindings = bindings;
            return ret;
        }
    }
}
