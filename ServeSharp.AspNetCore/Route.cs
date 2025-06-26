using System;
using System.Net.Http;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.AspNetCore;

public class Route
{
    public string Name { get; set; } = "UNNAMED";
    public string OriginalRouteDefinition { get; internal set; } = "";
    public Matcher Matcher { get; }
    public HttpMethod? Method { get; }
    private readonly HandleFunc<Context>[] _handlers;

    public Route(HttpMethod? method, Matcher matcher, params HandleFunc<Context>[] handlers)
    {
        Method = method;
        Matcher = matcher;
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
    }

    public override string ToString() => $"{Name} {Method?.ToString() ?? "ANY"} Handler[{_handlers.Length}] {OriginalRouteDefinition}";

    public bool Match(Context context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (context.Http.HttpContext == null)
        {
            throw new ArgumentNullException(nameof(context), "context.Http.HttpContext is null");
        }

        // method == null: match any
        if (Method != null)
        {
            // test method
            if (Method.ToString() != context.Http.HttpContext.Request.Method) return false;
        }

        // test path
        var ret = Matcher.Match(context.Http.HttpContext.Request.Path, out _, out var bindings);
        context.Http.UrlBindings = bindings;
        return ret;
    }

    public MiddlewareStack<Context> Stack => new MiddlewareStack<Context>(_handlers);
}

