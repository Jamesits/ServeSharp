#pragma warning disable CA1303
using ServeSharp.Core.Middleware;
using System.Collections.Concurrent;

namespace ServeSharp.Core.Test;

// Note: exception handling does not exist here, since we don't have a stack.
// Only pauses and continuations can be tested here.
public class MiddlewareTest
{
    [SetUp]
    public void Setup() { }

    [Test]
    public async Task TestPlainTerminatingTasks()
    {
        await ExecuteMiddlewareStack([
            PlainChainedMiddleware1,
            PlainChainedMiddleware2,
            PlainTerminatingMiddleware1
        ], [101, 102, 103, 201, 301, 302, 303, 304, 209, 208, 109]).ConfigureAwait(false);
    }

    private static async Task ExecuteMiddlewareStack(Func<StackingAwaiter, ConcurrentQueue<int>, Middleware.Middleware>[] stack, int[] expectedSeq)
    {
        var resultQueue = new ConcurrentQueue<int>();
        var next = new StackingAwaiter();
        try
        {
            await using (next)
            {
                foreach (var i in stack)
                {
                    await i(next, resultQueue);
                }
            }
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

    private static async Middleware.Middleware PlainChainedMiddleware1(IAwaitable next, ConcurrentQueue<int> resultQueue)
    {
        resultQueue.Enqueue(101);
        await Task.Yield();
        resultQueue.Enqueue(102);
        await Task.Delay(1).ConfigureAwait(false);
        resultQueue.Enqueue(103);
        await next;
        resultQueue.Enqueue(109);
    }

    private static async Middleware.Middleware PlainChainedMiddleware2(IAwaitable next, ConcurrentQueue<int> resultQueue)
    {
        resultQueue.Enqueue(201);
        await next;
        resultQueue.Enqueue(209);
        await Task.Yield();
        resultQueue.Enqueue(208);
    }

    private static async Middleware.Middleware PlainTerminatingMiddleware1(IAwaitable next, ConcurrentQueue<int> resultQueue)
    {
        resultQueue.Enqueue(301);

        await F().ConfigureAwait(false);

        resultQueue.Enqueue(304);
        return;

        async Task F()
        {
            resultQueue.Enqueue(302);
            await Task.Delay(2).ConfigureAwait(false);
            resultQueue.Enqueue(303);
        }
    }
}