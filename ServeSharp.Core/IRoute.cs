using ServeSharp.Core.Middleware;

namespace ServeSharp.Core;

public interface IRoute<TContext>
{
    public MiddlewareStack<TContext> Stack { get; }
    public bool Match(TContext context);
}