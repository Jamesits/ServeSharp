#pragma warning disable CA1303
using System.Collections.Concurrent;
using ServeSharp.Core.Middleware;

namespace ServeSharp.Core.Test;

public class MiddlewareStackTest
{
    [SetUp]
    public void Setup() { }

    [Test]
    public async Task TestPlainMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainChainedMiddleware2,
                PlainTerminatingMiddlewareAsync
            ),
            [101, 201, 301, 209, 109]);

        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainChainedMiddleware2,
                PlainTerminatingMiddlewareSync
            ),
            [101, 201, 301, 209, 109]);
    }

    [Test]
    public async Task TestEarlyReturnMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainTerminatingMiddlewareAsync,
                PlainChainedMiddleware2
            ),
            [101, 301, 109]);

        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainTerminatingMiddlewareSync,
                PlainChainedMiddleware2
            ),
            [101, 301, 109]);
    }

    // If exception is thrown before `await next` call, further middleware call would be stopped immediately; exception should pop up to every executed middleware's bottom part.
    [Test]
    public Task TestThrowingBeforeMiddlewareStack()
    {
        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    ExceptionBeforeMiddleware,
                    PlainChainedMiddleware2,
                    PlainTerminatingMiddlewareAsync
                ),
                [101, 401]);
        });

        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    ExceptionBeforeMiddleware,
                    PlainChainedMiddleware2,
                    PlainTerminatingMiddlewareSync
                ),
                [101, 401]);
        });
        return Task.CompletedTask;
    }

    // If exception is thrown after `await next` call, exception should pop up to every executed middleware's bottom part.
    [Test]
    public Task TestThrowingAfterMiddlewareStack()
    {
        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    ExceptionAfterMiddleware,
                    PlainChainedMiddleware2,
                    PlainTerminatingMiddlewareAsync
                ),
                [101, 501, 201, 301, 209, 509]);
        });

        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    ExceptionAfterMiddleware,
                    PlainChainedMiddleware2,
                    PlainTerminatingMiddlewareSync
                ),
                [101, 501, 201, 301, 209, 509]);
        });
        return Task.CompletedTask;
    }

    // Recovery middleware should not disrupt a normal execution flow.
    [Test]
    public async Task TestRecoveryNothingMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                RecoveryMiddleware,
                PlainChainedMiddleware2,
                PlainTerminatingMiddlewareAsync
            ),
            [101, 601, 201, 301, 209, 607, 609, 109]);

        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                RecoveryMiddleware,
                PlainChainedMiddleware2,
                PlainTerminatingMiddlewareSync
            ),
            [101, 601, 201, 301, 209, 607, 609, 109]);
    }

    [Test]
    public async Task TestRecoveryBeforeMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    RecoveryMiddleware,
                    PlainChainedMiddleware2,
                    ExceptionBeforeMiddleware,
                    PlainTerminatingMiddlewareAsync
                ),
                [101, 601, 201, 401, 608, 609, 109]);
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                RecoveryMiddleware,
                PlainChainedMiddleware2,
                ExceptionBeforeMiddleware,
                PlainTerminatingMiddlewareSync
            ),
            [101, 601, 201, 401, 608, 609, 109]);
    }

    [Test]
    public async Task TestRecoveryAfterMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                RecoveryMiddleware,
                PlainChainedMiddleware2,
                ExceptionAfterMiddleware,
                PlainTerminatingMiddlewareAsync
            ),
            [101, 601, 201, 501, 301, 509, 608, 609, 109]);
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                RecoveryMiddleware,
                PlainChainedMiddleware2,
                ExceptionAfterMiddleware,
                PlainTerminatingMiddlewareSync
            ),
            [101, 601, 201, 501, 301, 509, 608, 609, 109]);
    }

    [Test]
    public async Task TestRecoveryTerminatingMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
            PlainChainedMiddleware1,
            RecoveryMiddleware,
            PlainChainedMiddleware2,
            ExceptionBeforeMiddleware
        ),
        [101, 601, 201, 401, 608, 609, 109]);
    }

    // When exception is thrown, `try` should be cancelled, then `finally` should run, then control flow returns to the upper middleware; exception should be rethrown.
    [Test]
    public Task TestFinalizingTask()
    {
        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    FinalizerMiddleware,
                    PlainChainedMiddleware2,
                    ExceptionBeforeMiddleware,
                    PlainTerminatingMiddlewareAsync
                ),
                [101, 701, 201, 401, 709]);
        });

        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    FinalizerMiddleware,
                    PlainChainedMiddleware2,
                    ExceptionAfterMiddleware,
                    PlainTerminatingMiddlewareAsync
                ),
                [101, 701, 201, 501, 301, 509, 709]);
        });

        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    FinalizerMiddleware,
                    PlainChainedMiddleware2,
                    ExceptionBeforeMiddleware
                ),
                [101, 701, 201, 401, 709]);

        });

        return Task.CompletedTask;
    }

    // If no more middleware is found, `await next` should have no effect
    [Test]
    public async Task TestStackOverrunOnTermination1MiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainChainedMiddleware2
            ),
            [101, 201, 209, 109]);
    }

    // Excessive `await next` calls in the same middleware should have no effect
    [Test]
    public async Task TestStackOverrun2MiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainChainedMiddleware2,
                NextMiddleware2
            ),
            [101, 201, 1001, 1002, 1003, 209, 109]);

        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainChainedMiddleware2,
                NextMiddleware2,
                PlainTerminatingMiddlewareSync
            ),
            [101, 201, 1001, 301, 1002, 1003, 209, 109]);
    }

    private async Middleware.Middleware Execute(MiddlewareStack<ConcurrentQueue<int>> stack,
        int[] expectedSeq)
    {
        var resultQueue = new ConcurrentQueue<int>();
        try
        {
            await using var next = new StackingAwaiter();
            await stack.Handle(resultQueue, next);
        }
        finally
        {
            Console.WriteLine("Expected: [{0}]", string.Join(", ", expectedSeq));
            Console.WriteLine("Actual: [{0}]", string.Join(", ", resultQueue));

            if (!expectedSeq.SequenceEqual(resultQueue))
            {
                Assert.Fail();
            }
        }
    }

    // Plain middleware which chains to the next middleware
    private static async Middleware.Middleware PlainChainedMiddleware1(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(101);
        await next;
        context.Enqueue(109);
    }

    // Plain middleware which chains to the next middleware (duplicated for some test fixtures)
    private static async Middleware.Middleware PlainChainedMiddleware2(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(201);
        await next;
        context.Enqueue(209);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    // Plain middleware that returns (does not call the next middleware)
    private static async Middleware.Middleware PlainTerminatingMiddlewareAsync(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(301);
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    // Plain middleware that returns (does not call the next middleware), using Middleware.CompletedTask
    private static Middleware.Middleware PlainTerminatingMiddlewareSync(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(301);
        return Middleware.Middleware.CompletedTask;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    // Middleware that throws before calling the next middleware
    private static async Middleware.Middleware ExceptionBeforeMiddleware(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(401);
        throw new InvalidOperationException();
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    // Middleware that throws after calling the next middleware
    private static async Middleware.Middleware ExceptionAfterMiddleware(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(501);
        await next;
        context.Enqueue(509);
        throw new InvalidOperationException();
    }

    // Middleware that catches all exceptions from every middleware below
    private static async Middleware.Middleware RecoveryMiddleware(ConcurrentQueue<int> context, IAwaitable next)
    {
        try
        {
            context.Enqueue(601);
            await next;
            context.Enqueue(607);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught exception in Recovery middleware: {ex}");
            context.Enqueue(608);
        }
        finally
        {
            context.Enqueue(609);
        }
    }

    // Middleware that has a try/finally stanza
    private static async Middleware.Middleware FinalizerMiddleware(ConcurrentQueue<int> context, IAwaitable next)
    {
        try
        {
            context.Enqueue(701);
            await next;
            context.Enqueue(708);
        }
        finally
        {
            context.Enqueue(709);
        }
    }

    // Middleware that call `await next` twice
    private static async Middleware.Middleware NextMiddleware2(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(1001);
        await next;
        context.Enqueue(1002);
        await next;
        context.Enqueue(1003);
    }
}