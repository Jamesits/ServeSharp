#nullable enable
using System.Net.Http;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.NetHttp
{
    public class Route
    {
        public string Name { get; internal set; }
        public string OriginalRouteDefinition { get; internal set; }
        public Matcher Matcher { get; internal set; }
        public HttpMethod Method { get; internal set; }
        public HandleFunc<Context> Handler { get; internal set; }

        public override string ToString() => $"{Name ?? "UNNAMED"} {Method} {OriginalRouteDefinition}";

        public bool Match(Context context)
        {
            return Matcher.Match(context.Request.RequestUri.AbsolutePath, out _, out _);
        }
    }
}