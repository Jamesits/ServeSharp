using System;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;

namespace ServeSharp.CefSharpCore;

public class Server : IDisposable, ISchemeHandlerFactory
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

    #region impl of ISchemeHandlerFactory

    public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
    {
        using var context = new Context();
        context.Http.Browser = browser;
        context.Http.Frame = frame;
        context.Http.SchemeName = schemeName;
        context.Http.Request = request;
        context.Http.ResourceHandler = new ResourceHandler();

        Task.Run(async () =>
        {
            await Router.Handle(context);
        }).GetAwaiter().GetResult();

        // do not dispose the ResourceHandler now
        var ret = context.Http.ResourceHandler;
        context.Http.ResourceHandler = null;
        return ret;
    }
    #endregion
}
