using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using System;
using System.Net.Http;
using ServeSharp.Core.Router;

namespace ServeSharp.CefSharpCore;

public class Route(HttpMethod? method, Matcher matcher, params HandleFunc<Context>[] handlers) : Route<Context>(method, matcher, handlers)
{
    public override bool Match(Context context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (context.Http.Request == null)
        {
            throw new ArgumentNullException(nameof(context), "context.Http.Request is null");
        }

        // method == null: match any
        if (Method != null)
        {
            // test method
            if (Method.ToString() != context.Http.Request.Method) return false;
        }

        // test path
        var url = new Uri(context.Http.Request.Url);
        var ret = Matcher.Match(url.AbsolutePath, out _, out var bindings);
        context.UrlBindings = bindings;
        return ret;
    }
}
