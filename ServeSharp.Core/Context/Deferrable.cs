using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ServeSharp.Core.Context
{
    public class Deferrable : IDisposable, IAsyncDisposable
    {
        private bool _completed;

        private readonly ConcurrentStack<Action> _completions = new ConcurrentStack<Action>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Defer(Action action)
        {
            lock (this)
            {
                if (_completed)
                {
                    throw new InvalidOperationException("Use of Deferrable after disposal");
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
                action();
            }
        }

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
