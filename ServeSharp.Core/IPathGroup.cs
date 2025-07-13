#nullable enable
using System.Net.Http;
using ServeSharp.Core.Middleware;

namespace ServeSharp.Core;

public interface IPathGroup<out TContext, TRoute>
{
    public bool AutoHead { get; set; }

    public void Use(params HandleFunc<TContext>[] handlers);

    public IPathGroup<TContext, TRoute> Group(string path);

    public TRoute Handle(HttpMethod? method, string path, params HandleFunc<TContext>[] handlers);
}