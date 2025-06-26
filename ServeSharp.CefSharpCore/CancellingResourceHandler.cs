using CefSharp;

namespace ServeSharp.CefSharpCore;

public class CancellingResourceHandler : ResourceHandler
{
    public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
    {
        callback.Dispose();
        return CefReturnValue.Cancel;
    }
}
