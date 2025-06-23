using ServeSharp.Core.Middleware;
using System.Collections.Concurrent;
using Task = System.Threading.Tasks.Task;

namespace ServeSharp.Core.Test
{
    public class MiddlewareTaskTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            await ExecuteMiddlewareStack([
                PlainChainedMiddleware1,
                PlainChainedMiddleware2,
                PlainTerminatingMiddleware1
            ], [101, 102, 103, 201, 301, 302, 303, 304, 209, 208, 109]);
        }

        private async Task ExecuteMiddlewareStack(Func<StackingAwaiter, ConcurrentQueue<int>, Middleware.Task>[] stack, int[] expectedSeq)
        {
            var resultQueue = new ConcurrentQueue<int>();
            var next = new StackingAwaiter();
            await using (next)
            {
                foreach (var i in stack)
                {
                    await i(next, resultQueue);
                }
            }

            Console.WriteLine("Expected: [{0}]", string.Join(", ", expectedSeq));
            Console.WriteLine("Actual: [{0}]", string.Join(", ", resultQueue));

            if (!expectedSeq.SequenceEqual(resultQueue))
            {
                Assert.Fail();
            }
        }

        private async Middleware.Task PlainChainedMiddleware1(IAwaitable next, ConcurrentQueue<int> resultQueue)
        {
            resultQueue.Enqueue(101);
            await Task.Yield();
            resultQueue.Enqueue(102);
            await Task.Delay(100);
            resultQueue.Enqueue(103);
            await next;
            resultQueue.Enqueue(109);
        }

        private async Middleware.Task PlainChainedMiddleware2(IAwaitable next, ConcurrentQueue<int> resultQueue)
        {
            resultQueue.Enqueue(201);
            await next;
            resultQueue.Enqueue(209);
            await Task.Yield();
            resultQueue.Enqueue(208);
        }

        private async Middleware.Task PlainTerminatingMiddleware1(IAwaitable next, ConcurrentQueue<int> resultQueue)
        {
            resultQueue.Enqueue(301);

            async Task f()
            {
                resultQueue.Enqueue(302);
                await Task.Delay(100);
                resultQueue.Enqueue(303);
            }
            await f();

            resultQueue.Enqueue(304);
        }
    }
}
