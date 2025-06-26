using CefSharp;

namespace ServeSharp.CefSharpCore;

public class ResourceHandler : CefSharp.ResourceHandler
{
    public bool Continue { get; set; } = true;
    // Process request and craft response.
    public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
    {
        if (Continue)
        {
            callback.Continue();
            return CefReturnValue.ContinueAsync;
        }

        callback.Dispose();
        return CefReturnValue.Cancel;
    }
}
