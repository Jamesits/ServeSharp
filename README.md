# ServeSharp

HTTP router and middleware in C# that does not depend on any web framework.

![Works - On My Machine](https://img.shields.io/badge/Works-On_My_Machine-2ea44f) ![Project Status - Premature](https://img.shields.io/badge/Project_Status-Premature-yellow)

Note: Before v1.0.0, APIs are due to heavy change.

## Usage

There are a lot HTTP request/response class implementations in the C#/.NET ecosystem, and they provide different interfaces. If you happened to use one that has been implemented by this project, just use it. Otherwise, use our core infrastructure to create a router for your request/response type in no time.

Implemented:

| Package | Request | Response | Implementation | Status |
| ------- | ------- | -------- | -------------- | ------ |
| [`System.Net.Http`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http) | [`HttpRequestMessage`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage) | [`HttpResponseMessage`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage) | [`ServeSharp.NetHttp`](/ServeSharp.NetHttp) | ✅ |
| [CefSharp.Core](https://github.com/cefsharp/CefSharp) | `CefSharp.IRequest` | `CefSharp.IResourceHandler` | [`ServeSharp.CefSharpCore`](/ServeSharp.CefSharpCore) | ✅ [Example Project](/ServeSharp.CefSharpCore.Example.WPF)|
| ASP.NET Core (Kestrel or HTTP.sys) | `Microsoft.AspNetCore.Http.Request` | `Microsoft.AspNetCore.Http.Response` | [`ServeSharp.AspNetCore`](/ServeSharp.AspNetCore) | ✅ [Example Project](/ServeSharp.AspNetCore.Example.Cli) |

### Using an Existing Server

See [ServeSharp.AspNetCore.Example.Cli](/ServeSharp.AspNetCore.Example.Cli) for an example of using the router with the Kestrel server.

### Using an Existing Router

```csharp
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
# import the implementation corresponding to your HTTP request/response type
using ServeSharp.NetHttp;

namespace MyServer;

private class MyServer {
    private Router _router = new();

    public MyServer() {
        // add some middlewares
        _router.Use(CustomLogger);

        // add some routes
        _router.Get("/", GetRoot);
    }

    public void SendRequest() {
        // construct a new request
        var msg = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        using var ctx = new Context();
        ctx.Http.Request = msg;
        await _router.Handle(ctx);
        // process ctx.Response
    }

    public async Middleware CustomLogger(Context context, IAwaitable next) {
        // do something before

        await next; // call the next middleware

        // do something after
    }

    public async Middleware GetRoot(Context context, IAwaitable _) {
        // context.Http.Response.Content = ...;
    }
}

```

### Porting a New Router

Steps:

- Implement a [`class Context`](/ServeSharp.NetHttp/Context.cs)
- Implement a [`class Route`](/ServeSharp.NetHttp/Route.cs)
- Implement a [`class RouteGroup`](/ServeSharp.NetHttp/RouteGroup.cs)
- Implement a [`class Router`](/ServeSharp.NetHttp/Router.cs)

## FAQs

### Why

C#/.NET have a lot good web frameworks, but there is not one HTTP router implementation that can run without the corresponding server (and a lot other dependencies). There are cases that we need a router without a server (e.g. when there is an embedded webview and you want to serve some custom resources). So I wrote one.

### `MissingMethodException`

If your project has sufficiently high .NET version, after adding this package, you may encounter runtime errors like:

```plaintext
System.MissingMethodException: Method not found: ...
```

This is because this package uses an older version of target framework for maximum compatibility, which might cause CLR to not find some methods when the function invocation passes through the middleware chain. To fix this, use the package-provided version of these functions (e.g. `dotnet add System.Text.Json`) rather than using the framework-provided one. [Here is an excellent article explaining why this happens](https://sergeyteplyakov.github.io/Blog/csharp/2024/03/21/Mythical_MissingMethodException.html).

## Thanks

This project is sponsored by [Yet Another AI Ltd.](https://www.yetanother.ai/).

The API is heavy influenced by [Flamego](https://flamego.dev/).
