#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServeSharp.Core.Middleware;

public delegate Task HandleFunc<in T>(T context, IAwaitable next);

#pragma warning disable CA1711
public class MiddlewareStack<T>
#pragma warning restore CA1711
{
    private readonly List<HandleFunc<T>> _handles = [];

    public MiddlewareStack() { }

    public MiddlewareStack(params HandleFunc<T>[] handles)
    {
        Add(handles);
    }

    public void Add(params HandleFunc<T>[] handles) => _handles.AddRange(handles);

    public async Task Handle(T context)
    {
        var topHalfAwaiters = new StackingAwaiter[_handles.Count];
        var bottomHalfTasks = new Task[_handles.Count];

        try
        {
            // top half
            for (var i = 0; i < _handles.Count; ++i)
            {
                try
                {
                    topHalfAwaiters[i] = new StackingAwaiter();
                    bottomHalfTasks[i] = _handles[i](context, topHalfAwaiters[i]);
                    await Task.WhenAny([
                        bottomHalfTasks[i]!,
                        topHalfAwaiters[i].CancellationToken.WaitAsync(),
                    ]).ConfigureAwait(false);

                    // early exit because middleware does not call await next
                    if (bottomHalfTasks[i].IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (i > 0)
                    {
                        // If an exception is thrown anywhere inside the top half of the invocation
                        // queue the exception back into the stack top function's `await` invocation
                        topHalfAwaiters[i - 1].QueueException(ex);
                        // and stop the handler chain
                        break;
                    }
                    else
                    {
                        throw (new AggregateException()).Append(ex)!;
                    }
                }
            }

            // bottom half
            for (var i = _handles.Count - 1; i >= 0; --i)
            {
                try
                {
                    if (topHalfAwaiters[i] == null) continue;

                    topHalfAwaiters[i].Continue();
                    await bottomHalfTasks[i].ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (i > 0)
                    {
                        topHalfAwaiters[i - 1].QueueException(ex);
                    }
                    else
                    {
                        throw (new AggregateException()).Append(ex)!;
                    }
                }
            }
        }
        finally
        {
            for (var i = 0; i < _handles.Count; ++i)
            {
                if (topHalfAwaiters[i] != null)
                {
                    await topHalfAwaiters[i].DisposeAsync().ConfigureAwait(false);
                    topHalfAwaiters[i] = null!;
                }
            }
        }
    }
}
