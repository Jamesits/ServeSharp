using System.Collections.Concurrent;
using ServeSharp.Core.Middleware;

namespace ServeSharp.Core.Test;

public class MiddlewareStackTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task TestPlainMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                PlainChainedMiddleware2,
                PlainTerminatingMiddleware
            ),
            [101, 201, 301, 209, 109]);
    }

    [Test]
    public Task TestThrowingBeforeMiddlewareStack()
    {
        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    ExceptionBeforeMiddleware,
                    PlainChainedMiddleware2,
                    PlainTerminatingMiddleware
                ),
                [101, 401]);
        });
        return Task.CompletedTask;
    }

    [Test]
    public Task TestThrowingAfterMiddlewareStack()
    {
        Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                    PlainChainedMiddleware1,
                    ExceptionAfterMiddleware,
                    PlainChainedMiddleware2,
                    PlainTerminatingMiddleware
                ),
                [101, 501, 201, 301, 209, 509]);
        });
        return Task.CompletedTask;
    }

    [Test]
    public async Task TestRecoveryNothingMiddlewareStack()
    {
        await Execute(new MiddlewareStack<ConcurrentQueue<int>>(
                PlainChainedMiddleware1,
                RecoveryMiddleware,
                PlainChainedMiddleware2,
                PlainTerminatingMiddleware
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
                    PlainTerminatingMiddleware
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
                PlainTerminatingMiddleware
            ),
            [101, 601, 201, 501, 301, 509, 608, 609, 109]);
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

    private static async Middleware.Middleware PlainChainedMiddleware1(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(101);
        await next;
        context.Enqueue(109);
    }

    private static async Middleware.Middleware PlainChainedMiddleware2(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(201);
        await next;
        context.Enqueue(209);
    }

    private static async Middleware.Middleware PlainTerminatingMiddleware(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(301);
    }

    private static async Middleware.Middleware ExceptionBeforeMiddleware(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(401);
        throw new InvalidOperationException();
    }

    private static async Middleware.Middleware ExceptionAfterMiddleware(ConcurrentQueue<int> context, IAwaitable next)
    {
        context.Enqueue(501);
        await next;
        context.Enqueue(509);
        throw new InvalidOperationException();
    }

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
            context.Enqueue(608);
        }
        finally
        {
            context.Enqueue(609);
        }
    }
}