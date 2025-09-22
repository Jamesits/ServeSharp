using System.Diagnostics.CodeAnalysis;
using System.IO;
using CefSharp;
using CefSharp.Wpf;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;

namespace ServeSharp.CefSharpCore.Example.WPF;

/// <summary>
/// Interaction logic for BrowserWindow.xaml
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
#pragma warning disable CA1515
public partial class BrowserWindow : IDisposable
#pragma warning restore CA1515
{
    private bool _disposed;
    private readonly CefSettings _cefSettings;
    private readonly BrowserSettings _browserSettings;

    public BrowserWindow()
    {
        var router = new Router();
        router.Get("/", (context, _) =>
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            context.Http.ResourceHandler = new ContinuingResourceHandler()
            {
                StatusCode = 200,
                MimeType = "text/html",
                Stream = new MemoryStream("<h1>It works!</h1>"u8.ToArray()),
            };
            return Task.CompletedTask;
        });

        _cefSettings = new CefSettings();
        _cefSettings.RegisterScheme(new CefCustomScheme
        {
            SchemeName = "res",
            SchemeHandlerFactory = router,
        });
        Cef.Initialize(_cefSettings);

        InitializeComponent();

        _browserSettings = new BrowserSettings
        {
            WindowlessFrameRate = 60,
        };
        Browser.BrowserSettings = _browserSettings;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            Cef.Shutdown();
            _browserSettings.Dispose();
            _cefSettings.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~BrowserWindow()
    {
        Dispose(false);
    }
}
