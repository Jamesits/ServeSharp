using System.Threading;
using System.Threading.Tasks;

namespace ServeSharp.Core.Middleware
{
    public static class CancellationTokenExtensions
    {
        public static async Task WaitAsync(this CancellationToken ct)
        {
            await Task.WhenAny(Task.Delay(Timeout.Infinite, ct)).ConfigureAwait(false);
        }
    }
}
