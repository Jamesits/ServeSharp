#nullable enable
using System;
using System.Collections.Generic;

namespace ServeSharp.Core.Middleware;

public delegate Middleware HandleFunc<in T>(T context, IAwaitable next);

public class MiddlewareStack<T>
{
    private readonly List<HandleFunc<T>> _handles = [];

    public MiddlewareStack() { }

    public MiddlewareStack(params HandleFunc<T>[] handles)
    {
        Add(handles);
    }

    public void Add(params HandleFunc<T>[] handles) => _handles.AddRange(handles);

    public async Middleware Handle(T context, StackingAwaiter next)
    {
        if (next == null) throw new ArgumentNullException(nameof(next));

        foreach (var h in _handles)
        {
            try
            {
                next.StackPush();
                await h(context, next);
                if (!next.StackPop())
                {
                    // Detect early return from the middleware stack and stop processing
                    break;
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                // If an exception is thrown anywhere inside the top half of the invocation
                // queue the exception back into the stack top function's `await` invocation
                next.QueueException(ex);
                // and stop the handler chain
                break;
            }
        }
    }
}
