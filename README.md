# ServeSharp

HTTP router and Task in C# that does not depend on any web framework.

![Works - On My Machine](https://img.shields.io/badge/Works-On_My_Machine-2ea44f) ![Project Status - Premature](https://img.shields.io/badge/Project_Status-Premature-yellow)

Note: Before v1.0.0, APIs are due to heavy change.

## Usage

### Choose an Implementation Path

ServeSharp can be used as a standalone HTTP server, a Task for an existing HTTP server, an in-process server for a webview frontend, or a router for any RPC-like request/response interface.

There are a lot HTTP request/response class implementations in the C#/.NET ecosystem, and they provide different interfaces. If you happened to use one listed below, use the specific package. Otherwise, use `ServeSharp.Core` to create a router for your request/response type in less than 50 lines of code.

| Package | Implementation |
| ------- | -------------- |
| [`System.Net.Http`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http) | [`ServeSharp.NetHttp`](/ServeSharp.NetHttp) |
| [`System.Net`](https://learn.microsoft.com/en-us/dotnet/api/system.net) | [`ServeSharp.NetHttpListener`](/ServeSharp.NetHttpListener) |
| [`CefSharp`](https://github.com/cefsharp/CefSharp) | [`ServeSharp.CefSharpCore`](/ServeSharp.CefSharpCore) |
| [`Microsoft.AspNetCore.Http`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http) | [`ServeSharp.AspNetCore`](/ServeSharp.AspNetCore) |
| [`HttpMachine.PCL`](https://github.com/1iveowl/HttpMachine.PCL) | [`ServeSharp.HttpMachine`](/ServeSharp.HttpMachine) |

<details>
<summary>Using an Existing Server</summary>

- Kestrel web server or HTTP.sys: [ServeSharp.AspNetCore.Example.Cli](/ServeSharp.AspNetCore.Example.Cli)

</details>

<details>
<summary>Using an Existing Embedding Option</summary>

- CefSharp with custom resource handler: [ServeSharp.CefSharpCore.Example.WPF](/ServeSharp.CefSharpCore.Example.WPF)

</details>

<details>
<summary>Using an Existing Router</summary>

```csharp
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
// import the implementation corresponding to your HTTP request/response type
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
        await _router.ServeHttp(ctx);
        // process ctx.Response
    }

    public async Task CustomLogger(Context context, IAwaitable next) {
        // do something before

        await next; // call the next middleware

        // do something after
    }

    public async Task GetRoot(Context context, IAwaitable _) {
        // context.Http.Response.Content = ...;
    }
}

```

</details>

<details>
<summary>Porting a New Router</summary>

Steps:

- Implement a [`class Context`](/ServeSharp.NetHttp/Context.cs)
- Implement a [`class Route`](/ServeSharp.NetHttp/Route.cs)
- Implement a [`class RouteGroup`](/ServeSharp.NetHttp/RouteGroup.cs)
- Implement a [`class Router`](/ServeSharp.NetHttp/Router.cs)

</details>

### Middleware

When a route is matched, the router will pass the context to a chain of middlewares. Each Task can do something before and after the next Task is called. A route handler is also a middleware.

Use `Router.Use(middleware1, [middleware2, ...])` to add middlewares to the router. The middlewares will be executed in the order they are added. Task is only effective for the routes that are added after it.

<details>
<summary>Route Handler</summary>

```csharp
// async route handler
public async Task GetRoot(Context context, IAwaitable _) {
    // set the response content
    context.Http.Response.Content = await Task.FromResult("Hello, world!");
}

// sync route handler
public Task GetRoot(Context context, IAwaitable _) {
    // set the response content
    context.Http.Response.Content = "Hello, world!";

    // no need to call next, as this is the end of the Task chain
    return Middleware.CompletedTask;
}
```

</details>

<details>
<summary>Logger Middleware</summary>

```csharp
// async middleware
public async Task CustomLogger(Context context, IAwaitable next) {
    // do something before
    Console.WriteLine("Before");

    // call the next middleware
    await next;

    // do something after
    Console.WriteLine("After");
}

// sync middleware
public Task CustomLogger(Context context, IAwaitable next) {
    // do something before
    Console.WriteLine("Before");

    // call the next middleware
    next.GetAwaiter().GetResult();

    // do something after
    Console.WriteLine("After");

    return Middleware.CompletedTask;
}
```

</details>

<details>
<summary>Exception Handling (Recovery) Middleware</summary>

If any Task throws an exception, the exception is wrapped in an `AggregatedException`(to preserve the original stack informat) then bubbled up to every Task in the chain on top of the one that threw the exception. You can catch and handle the exception [as described in the documentation](https://learn.microsoft.com/en-us/dotnet/api/system.aggregateexception.flatten#examples).

```csharp
public async Task Recovery(Context context, IAwaitable next) {
    try {
        await next;
    } catch (AggregateException ex) {
        // handle the exception
        foreach (var e in ex.Flatten().InnerExceptions) {
            Console.WriteLine(e.Message);
        }
    }
}
```

</details>

### Routing

Add routes to the router using `Router.Get`, `Router.Post`, `Router.Put`, `Router.Delete`, `Router.Any`, etc. Each route can have a path and a handler middleware. The path can contain parameters, which will be extracted from the request URL when the route is matched.

Supported formats:

- `/path/to/resource`: exact match
- `/path/{param}`: parameter will match any string until the next `/` or the end of the path
- `/path/{param?}`: optional match
- `/path/{param1}-{param2}-{param3}`: parameters will match non-greedy segments separated by your custom separator (e.g. `-` in this case)
- `/path/{param: splat}`: parameter will match any string until the end of the path
- `/path/{param: splat(N)}`: parameter will match N segments separated by `/`
- `/path/{param: /regex/}`: parameter will match the regex pattern (including any `/` if the regex permits)

You can read the parameter values from `context.Http.UrlBindings`.

If mulitple rules match the same request, which one is used is undefined.

## FAQs

### Why

C#/.NET have a lot good web frameworks, but there is not one HTTP router implementation that can run without the corresponding server (and a lot other dependencies). There are cases that we need a router without a server (e.g. when there is an embedded webview and you want to serve some custom resources). So I wrote one.

### `MissingMethodException`

If your project has sufficiently high .NET version, after adding this package, you may encounter runtime errors like:

```plaintext
System.MissingMethodException: Method not found: ...
```

This is because this package uses an older version of target framework for maximum compatibility, which might cause CLR to not find some methods when the function invocation passes through the Task chain. To fix this, use the package-provided version of these functions (e.g. `dotnet add System.Text.Json`) rather than using the framework-provided one. [Here is an excellent article explaining why this happens](https://sergeyteplyakov.github.io/Blog/csharp/2024/03/21/Mythical_MissingMethodException.html).

## Acknowledgements

This project is sponsored by [Yet Another AI Ltd.](https://www.yetanother.ai/).

The API is heavy influenced by [Flamego](https://flamego.dev/).
