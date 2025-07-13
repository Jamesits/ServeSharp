using System;
using System.Net.Http;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using ServeSharp.Core.Router;

namespace ServeSharp.AspNetCore;

public class Route(HttpMethod? method, Matcher matcher, params HandleFunc<Context>[] handlers)
    : Route<Context>(method, matcher, handlers)
{
    public override bool Match(Context context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (context.Http.HttpContext == null)
        {
            throw new ArgumentNullException(nameof(context), "context.Http.HttpContext is null");
        }

        // method == null: match any
        if (Method != null)
        {
            // test method
            if (Method.ToString() != context.Http.HttpContext.Request.Method) return false;
        }

        // test path
        var ret = Matcher.Match(context.Http.HttpContext.Request.Path, out _, out var bindings);
        context.UrlBindings = bindings;
        return ret;
    }
}
