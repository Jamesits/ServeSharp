#nullable enable
using System.Net.Http;
using ServeSharp.Core.Middleware;

namespace ServeSharp.Core.Path;

public interface IPathGroup<out TContext, out TRoute>
{
    public bool AutoHead { get; set; }

    public void Use(params HandleFunc<TContext>[] handlers);

    public IPathGroup<TContext, TRoute> Group(string path);

    public TRoute Route(HttpMethod? method, string path, params HandleFunc<TContext>[] handlers);
}