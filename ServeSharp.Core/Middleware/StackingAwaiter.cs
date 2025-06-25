#nullable enable
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ServeSharp.Core.Middleware
{
    /// <summary>
    /// StackingAwaiter, when awaited, folds all the code after the await line into a stack. When it is disposed, the deferred code are executed in reverse order.
    /// Deferred execution is synchronous. The `async` grammar is only used as a synthetic sugar.
    /// </summary>
    public class StackingAwaiter : IAwaiter, IAwaitable, ICriticalNotifyCompletion, IDisposable, IAsyncDisposable
    {
        // Prevents invocation after disposal
        private bool _completed;
        // Keeps track of any deferred code blocks
        private readonly ConcurrentStack<Action> _completions = new ConcurrentStack<Action>();
        // Queued exception to be raised on `await`
        private AggregateException? _exception;

        // Queue an exception to be raised at next await
        public void QueueException(Exception? exception)
        {
            lock (this)
            {
                // Always save the original exception inside one AggregateException, since stack information is wiped if the exception is re-thrown.
                _exception = _exception.Append(exception);
            }
        }

        // Manually add functions to the defer stack
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Defer(Action action)
        {
            lock (this)
            {
                if (_completed)
                {
                    throw new InvalidOperationException("Use of StackingAwaiter after disposal");
                }
                _completions.Push(action);
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
            }

            while (true)
            {
                var ok = _completions.TryPop(out var action);
                if (!ok) break;

                // Notes on exception handling
                try
                {
                    // The action starts from this.GetResult(), so if there is one pending exception, the action will immediately receive the exception.
                    action();
                }
                catch (Exception ex)
                {
                    // If the action did not do try {await xxx}, the exception will be back here, so we can try the next action.
                    // If the exception is not handled anywhere inside the action stack, it will bubble back to the function where this object is disposed.
                    QueueException(ex);
                }
            }

            // throw any exception that has not been processed by the deferred code blocks
            GetResult();
        }
        #region impl of ICriticalNotifyCompletion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action completion) => Defer(completion);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action completion) => Defer(completion);

        #endregion

        #region impl of Middleware type (Awaitable expressions)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAwaiter GetAwaiter() => this;

        #endregion

        #region impl of Awaiter

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

        #endregion

        #region impl of IDisposable and IAsyncDisposable

        // impl of IDisposable and IAsyncDisposable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => ContinueAll();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask DisposeAsync() => ContinueAll();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #endregion
    }
}