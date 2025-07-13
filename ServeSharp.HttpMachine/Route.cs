using System;
using System.Net.Http;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using ServeSharp.Core.Router;

namespace ServeSharp.HttpMachine;

public class Route(HttpMethod? method, Matcher matcher, params HandleFunc<Context>[] handlers) : Route<Context>(method, matcher, handlers)
{
    public override bool Match(Context context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (context.Request == null)
        {
            throw new ArgumentNullException(nameof(context), "context.Request is null");
        }

        // method == null: match any
        if (Method != null)
        {
            // test method
            if (Method != new HttpMethod(context.Request.Method)) return false;
        }

        // test path
        var ret = Matcher.Match(context.Request.Path, out _, out var bindings);
        context.UrlBindings = bindings;
        return ret;
    }
}