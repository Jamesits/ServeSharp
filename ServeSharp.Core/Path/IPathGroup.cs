using System.Net.Http;
using ServeSharp.Core.Middleware;

namespace ServeSharp.Core.Path
{
    public interface IPathGroup<out TContext, out TRoute>
    {
        public bool AutoHead { get; set; }

        // currently not inheritable
        // public HandleFunc<TContext> NotFound { set; }

        public void Use(params HandleFunc<TContext>[] middleware);
        public IPathGroup<TContext, TRoute> Group(string path);

        public TRoute Route(HttpMethod method, string path, HandleFunc<TContext> handler);
        public TRoute Get(string path, HandleFunc<TContext> handler);
        public TRoute Patch(string path, HandleFunc<TContext> handler);
        public TRoute Post(string path, HandleFunc<TContext> handler);
        public TRoute Put(string path, HandleFunc<TContext> handler);
        public TRoute Delete(string path, HandleFunc<TContext> handler);
        public TRoute Options(string path, HandleFunc<TContext> handler);
        public TRoute Head(string path, HandleFunc<TContext> handler);
        public TRoute Trace(string path, HandleFunc<TContext> handler);
    }
}
