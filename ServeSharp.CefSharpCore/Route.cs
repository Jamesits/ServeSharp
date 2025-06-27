using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using System;
using System.Net.Http;

namespace ServeSharp.CefSharpCore;

public class Route(HttpMethod? method, Matcher matcher, params HandleFunc<Context>[] handlers)
{
    public string Name { get; set; } = "UNNAMED";
    public string OriginalRouteDefinition { get; internal set; } = "";
    public Matcher Matcher { get; } = matcher;
    public HttpMethod? Method { get; } = method;
    private readonly HandleFunc<Context>[] _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));

    public override string ToString() => $"{Name} {Method?.ToString() ?? "ANY"} Handler[{_handlers.Length}] {OriginalRouteDefinition}";

    public bool Match(Context context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (context.Http.Request == null)
        {
            throw new ArgumentNullException(nameof(context), "context.Http.Request is null");
        }

        // method == null: match any
        if (Method != null)
        {
            // test method
            if (Method.ToString() != context.Http.Request.Method) return false;
        }

        // test path
        var url = new Uri(context.Http.Request.Url);
        var ret = Matcher.Match(url.AbsolutePath, out _, out var bindings);
        context.Http.UrlBindings = bindings;
        return ret;
    }

    public MiddlewareStack<Context> Stack => new MiddlewareStack<Context>(_handlers);
}

