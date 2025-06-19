#nullable enable
using System;
using System.Collections.Generic;

namespace ServeSharp.Core.Middleware
{
    public delegate Middleware HandleFunc<in T>(T context, DeferrableAwaiter next);

    public class HandlerStack<T>
    {
        private readonly List<HandleFunc<T>> _handles = new List<HandleFunc<T>>();

        public HandlerStack() { }

        public HandlerStack(params HandleFunc<T>[] handles)
        {
            _handles.AddRange(handles);
        }

        public void Add(params HandleFunc<T>[] handles) => _handles.AddRange(handles);

        public async Middleware Handle(T context, DeferrableAwaiter next)
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
