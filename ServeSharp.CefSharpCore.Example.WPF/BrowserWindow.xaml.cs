using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
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
public partial class BrowserWindow : Window, IDisposable
#pragma warning restore CA1515
{
    private bool _disposed;
    private readonly Server _server;
    private readonly CefSettings _settings;

    public BrowserWindow()
    {
        _server = new Server();
        _server.Router.Get("/", (context, _) =>
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(context.Http.ResourceHandler, nameof(context.Http.ResourceHandler));

            context.Http.ResourceHandler.Continue = true;
            context.Http.ResourceHandler.StatusCode = 200;
            context.Http.ResourceHandler.MimeType = "text/html";
            context.Http.ResourceHandler.Stream = new MemoryStream("<h1>It works!</h1>"u8.ToArray());
            return Middleware.CompletedTask;
        });

        _settings = new CefSettings()
        {
            LogSeverity = LogSeverity.Verbose,
        };
        _settings.RegisterScheme(new CefCustomScheme
        {
            SchemeName = "res",
            SchemeHandlerFactory = _server,
        });
        Cef.Initialize(_settings);

        InitializeComponent();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            Cef.Shutdown();
            _settings.Dispose();
            _server?.Dispose();
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
