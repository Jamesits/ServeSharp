using System.Net.Http;
using ServeSharp.Core.Middleware;

namespace ServeSharp.Core.Path;

public static class PathGroupExtension
{
    public static TRoute Any<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(null, path, handler);
    public static TRoute Get<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler)
    {
        if (group.AutoHead)
        {
            group.Handle(HttpMethod.Head, path, handler);
        }

        return group.Handle(HttpMethod.Get, path, handler);
    }
    public static TRoute Patch<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(HttpMethod.Patch, path, handler);
    public static TRoute Post<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(HttpMethod.Post, path, handler);
    public static TRoute Put<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(HttpMethod.Put, path, handler);
    public static TRoute Delete<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(HttpMethod.Delete, path, handler);
    public static TRoute Options<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(HttpMethod.Options, path, handler);
    public static TRoute Head<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(HttpMethod.Head, path, handler);
    public static TRoute Trace<TContext, TRoute>(this IPathGroup<TContext, TRoute> group, string path, HandleFunc<TContext> handler) => group.Handle(HttpMethod.Trace, path, handler);
}