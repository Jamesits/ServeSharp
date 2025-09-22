#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ServeSharp.Core.Middleware;

/// <summary>
/// StackingAwaiter, when awaited, folds all the code after the await line into a stack. When it is disposed, the deferred code are executed in reverse order.
/// Deferred execution is synchronous. The `async` grammar is only used as a synthetic sugar.
/// </summary>
public sealed class StackingAwaiter : IAwaiter, IAwaitable, ICriticalNotifyCompletion, IDisposable, IAsyncDisposable
{
    private Stack<Action> _deferStack = new();
    // Queued exception to be raised on `await`
    private AggregateException? _exception;
    private CancellationTokenSource _ctsTopHalfDone = new();

    public CancellationToken CancellationToken => _ctsTopHalfDone.Token;

    public void Continue()
    {
        _ctsTopHalfDone.Cancel();
        while (_deferStack.Count > 0)
        {
            var c = _deferStack.Pop();
            c();
        }
    }

    // Queue an exception to be raised at next await
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void QueueException(Exception? exception)
    {
        // Always save the original exception inside one AggregateException, since stack information is wiped if the exception is re-thrown.
        _exception = _exception.Append(exception);
    }

    // Manually add functions to the defer stack
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Defer(Action action)
    {
        var capturedContext = SynchronizationContext.Current;
        _deferStack.Push(() =>
        {
            if (capturedContext != null)
                capturedContext.Post(_ => action(), null);
            else
                action();
        });
        _ctsTopHalfDone.Cancel();
    }
    #region impl of ICriticalNotifyCompletion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) => Defer(continuation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action continuation) => Defer(continuation);

    #endregion

    #region impl of Task type (Awaitable expressions)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IAwaiter GetAwaiter() => this;

    #endregion

    #region impl of Awaiter

    // IsCompleted should always return false unless the object has been disposed, so that we can stash the following state machine invocation into our stack.
    public bool IsCompleted => false;
    // Signal that we are done immediately
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetResult()
    {
        _ctsTopHalfDone.Token.WaitHandle.WaitOne();
        if (_exception != null)
        {
            var ex = _exception;
            _exception = null;
            throw ex;
        }
    }
    #endregion

    #region impl of IDisposable

    public void Dispose()
    {
        _ctsTopHalfDone.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(Dispose).ConfigureAwait(false);
    }
    #endregion
}
