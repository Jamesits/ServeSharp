#nullable enable
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ServeSharp.Core.Middleware
{    
    // DeferrableAwaiter, when awaited, folds all the code after the await line into a stack. When it is disposed,
    // the deferred code are executed in reverse order. 
    // Deferred execution is synchronous. The `async` grammar is only used as a synthetic sugar.
    public class DeferrableAwaiter : ICriticalNotifyCompletion, IDisposable, IAsyncDisposable
    {
        // Prevents invocation after disposal
        private bool _completed;
        // Keeps track of any deferred code blocks
        private readonly ConcurrentStack<Action> _completions = new ConcurrentStack<Action>();
        // Queued exception to be raised on `await`
        private Exception? _exception;

        // Manually add functions to the defer stack
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Defer(Action action)
        {
            lock (this)
            {
                if (_completed)
                {
                    throw new InvalidOperationException("Use of DeferrableAwaiter after disposal");
                }
                _completions.Push(action);
            }
        }

        // Execute one deferred action on the top of the stack
        // This is required if you want to control exception propagation rather than just throw onto the initial caller of the async task chain.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void StepUnwind()
        {
            lock (this)
            {
                var ok = _completions.TryPop(out var action);
                if (!ok) throw new InvalidOperationException("Nothing to unwind");
                action();
            }
        }

        // Execute the deferred code blocks and mark the object as disposed
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ContinueAll()
        {
            lock (this)
            {
                if (_completed) return;
                _completed = true;

                while (true)
                {
                    var ok = _completions.TryPop(out var action);
                    if (!ok) break;
                    action();
                }
            }
        }

        // impl of ICriticalNotifyCompletion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action completion) => Defer(completion);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action completion) => Defer(completion);

        // impl of Task type (Awaitable expressions)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeferrableAwaiter GetAwaiter() => this;

        // impl of Awaiter
        // IsCompleted should always return false unless the object has been disposed, so that we can stash the following state machine invocation into our stack.
        public bool IsCompleted
        {
            get
            {
                lock (this)
                {
                    return _completed;
                }
            }
        }
        // Signal that we are done immediately
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult()
        {
            lock (this)
            {
                var ex = _exception;
                _exception = null;
                if (ex != null)
                {
                    throw ex;
                }
            }
        }
        
        // impl of IDisposable and IAsyncDisposable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => ContinueAll();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask DisposeAsync() => ContinueAll();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        // Queue an exception to be raised at next await
        public void QueueException(Exception? exception)
        {
            lock (this)
            {
                _exception = exception;
            }
        }
    }
}