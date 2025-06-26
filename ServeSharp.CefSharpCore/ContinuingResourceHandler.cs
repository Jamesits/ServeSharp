using CefSharp;

namespace ServeSharp.CefSharpCore;

public class ContinuingResourceHandler : ResourceHandler
{
    // Process request and craft response.
    public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
    {
        callback.Continue();
        return CefReturnValue.ContinueAsync;
    }
}
