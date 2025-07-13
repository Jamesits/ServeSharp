#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;

namespace ServeSharp.Core.Middleware;

/// <summary>
/// Class <c>Middleware</c>
/// </summary>
// Middleware is a Middleware-like Awaiter that implements a function call chain where parent functions can execute anything before and after the child functions.
[AsyncMethodBuilder(typeof(MiddlewareAsyncMethodBuilder))]
public class Middleware : IAwaitable, IAwaiter, ICriticalNotifyCompletion
{
    // Signals the end of the "before" hook
    private readonly CancellationToken _ctTopHalf;
    // Signals the end of the "after" hook; unused for now
    private readonly CancellationToken _ctLowerHalf;
    // Contains the exception from the "before" hook; required for Awaiter to bubble up the exception to the caller
    private Exception? _exception;

    internal Middleware() { }

    internal Middleware(CancellationToken ctTopHalf, CancellationToken ctLowerHalf)
    {
        _ctTopHalf = ctTopHalf;
        _ctLowerHalf = ctLowerHalf;
    }

    // Useful if you have one sync Middleware function and want to get rid of the compiler warnings.
    public static Middleware CompletedTask => new Middleware { IsCompleted = true };

    // IsCompleted should always return false, so that the parent AsyncMethodBuilder calls our OnCompleted method.
    public bool IsCompleted { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IAwaiter GetAwaiter() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetResult()
    {
        lock (this)
        {
            if (_exception != null)
            {
                throw _exception;
            }

            if (IsCompleted)
            {
                return;
            }
        }
        _ctTopHalf.WaitHandle.WaitOne();
    }

    // Used by AsyncMethodBuilder
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetException(Exception exception)
    {
        lock (this)
        {
            _exception = exception;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Exception? GetException()
    {
        lock (this)
        {
            return _exception;
        }
    }

    // Execute anything after us in the current thread
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action completion) => completion();

    // Execute anything after us in the current thread
    [SecuritySafeCritical]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action completion) => completion();
}

public class MiddlewareAsyncMethodBuilder
{
    private readonly CancellationTokenSource _ctsTopHalf = new CancellationTokenSource();
    private readonly CancellationTokenSource _ctsLowerHalf = new CancellationTokenSource();

    public MiddlewareAsyncMethodBuilder()
    {
        Task = new Middleware(_ctsTopHalf.Token, _ctsLowerHalf.Token);
    }
    public Middleware Task { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MiddlewareAsyncMethodBuilder Create() => new MiddlewareAsyncMethodBuilder();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();

    // Not used when we are a class
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA1822
    // ReSharper disable once UnusedMember.Global
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
#pragma warning restore CA1822

    // Will be called if there is an exception thrown in the async function
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetException(Exception exception)
    {
        Task.SetException(exception);
        SignalBottomHalfDone();
    }

    // Will be called if the async function being awaited finished successfully.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult()
    {
        SignalBottomHalfDone();
    }

    // Signal that we reached an `await next`
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SignalTopHalfDone(StackingAwaiter da, Action completion)
    {
        lock (this)
        {
            if (_ctsTopHalf.IsCancellationRequested)
            {
                throw new InvalidOperationException("Can't `await next` twice");
            }

            // queue the lower half
            da.Defer(() =>
            {
                completion();
                _ctsLowerHalf.Token.WaitHandle.WaitOne();
            });

            // signal the await to return now
            _ctsTopHalf.Cancel();
        }
    }

    // Signal that the function finished.
    // Note: This method might be invoked twice if the middleware does not call await Next.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SignalBottomHalfDone()
    {
        lock (this)
        {
            _ctsTopHalf.Cancel();
            _ctsLowerHalf.Cancel();
            var ex = Task.GetException();
            if (ex != null)
            {
                throw ex;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is StackingAwaiter da)
        {
            SignalTopHalfDone(da, stateMachine.MoveNext);
        }
        else
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }
    }

    [SecuritySafeCritical]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is StackingAwaiter da)
        {
            SignalTopHalfDone(da, stateMachine.MoveNext);
        }
        else
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }
}