#nullable enable
using System;
using System.Collections.Generic;

namespace ServeSharp.Core.Middleware
{
    public delegate Middleware HandleFunc(Context context, DeferrableAwaiter next);

    public delegate bool ExceptionHandleFunc(Exception ex);

    public class HandlerStack
    {
        private readonly List<HandleFunc> _handles = new List<HandleFunc>();

        public HandlerStack() { }

        public HandlerStack(params HandleFunc[] handles)
        {
            _handles.AddRange(handles);
        }

        public void Add(HandleFunc handle) => _handles.Add(handle);

        public async Middleware Handle(Context context, DeferrableAwaiter next)
        {
            Exception? e = null;
            var stackDepth = 0;

            foreach (var h in _handles)
            {
                try
                {
                    await h(context, next);
                    stackDepth += 1;
                }
                catch (Exception ex)
                {
                    e = ex;
                    break;
                }
            }

            for (; stackDepth > 0; stackDepth--)
            {
                try
                {
                    next.QueueException(e);
                    e = null;
                    next.StepUnwind();
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }
        }
    }
}
