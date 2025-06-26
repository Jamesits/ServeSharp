using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServeSharp.AspNetCore;

public class Server : IDisposable
{
    private bool _disposed;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public Router Router { get; } = new Router();

    #region impl of IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _cts.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion

    public void Serve(HttpContext context)
    {
        using var ctx = new Context();
        ctx.Http.HttpContext = context;

        Task.Run(async () =>
        {
            await Router.Handle(ctx);
        }).GetAwaiter().GetResult();

        // prevent context being disposed
        ctx.Http.HttpContext = null;
    }
}
