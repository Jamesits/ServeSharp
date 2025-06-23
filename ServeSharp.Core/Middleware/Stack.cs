#nullable enable
using System;
using System.Collections.Generic;

namespace ServeSharp.Core.Middleware
{
    public delegate Task HandleFunc<in T>(T context, IAwaitable next);

    public class Stack<T>
    {
        private readonly List<HandleFunc<T>> _handles = new List<HandleFunc<T>>();

        public Stack() { }

        public Stack(params HandleFunc<T>[] handles)
        {
            _handles.AddRange(handles);
        }

        public void Add(params HandleFunc<T>[] handles) => _handles.AddRange(handles);

        public async Task Handle(T context, StackingAwaiter next)
        {
            foreach (var h in _handles)
            {
                try
                {
                    await h(context, next);
                }
                catch (Exception ex)
                {
                    // If an exception is thrown anywhere inside the top half of the invocation, stop the handler chain 
                    next.QueueException(ex);
                    break;
                }
            }
        }
    }
}
