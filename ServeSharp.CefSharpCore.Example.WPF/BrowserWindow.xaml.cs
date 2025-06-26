using System.Windows;
// using CefSharp;
// using CefSharp.Wpf;

namespace ServeSharp.CefSharpCore.Example.WPF;

internal partial class BrowserWindow : Window
{
    // private bool _disposed;
    // private readonly Server _server;
    // private readonly CefSettings _settings;

    // public BrowserWindow()
    // {
    //     _server = new Server();

    //     _settings = new CefSettings();
    //     _settings.RegisterScheme(new CefCustomScheme
    //     {
    //         SchemeName = "res",
    //         SchemeHandlerFactory = _server,
    //     });
    //     Cef.Initialize(_settings);
    // }

    // protected virtual void Dispose(bool disposing)
    // {
    //     if (_disposed) return;
    //     if (disposing)
    //     {
    //         Cef.Shutdown();
    //         _settings.Dispose();
    //         _server?.Dispose();
    //     }

    //     _disposed = true;
    // }

    // public void Dispose()
    // {
    //     Dispose(true);
    //     GC.SuppressFinalize(this);
    // }

    // ~BrowserWindow()
    // {
    //     Dispose(false);
    // }
}
