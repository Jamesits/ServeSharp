#nullable enable
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using System.Net.Http;
using System;

namespace ServeSharp.Core.Router;

public abstract class Route<TContext>(HttpMethod? method, Matcher matcher, params HandleFunc<TContext>[] handlers) : IRoute<TContext>
{
    public string Name { get; set; } = "UNNAMED";
    public string OriginalRouteDefinition { get; set; } = "";
    public Matcher Matcher { get; } = matcher;
    public HttpMethod? Method { get; } = method;
    private readonly HandleFunc<TContext>[] _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));

    public override string ToString() => $"{Name} {Method?.ToString() ?? "ANY"} Handler[{_handlers.Length}] {OriginalRouteDefinition}";

    public MiddlewareStack<TContext> Stack => new (_handlers);

    public abstract bool Match(TContext context);
}