# ServeSharp.Core

ServeSharp.Core is a simple and flexible utility library for building HTTP servers or generic request-response RPC servers.

## Context

The `Context` class wraps a Dictionary of string keys and object values. It provides a `.Get<T>()` method to disguise itself as a typed object with auto-generated getter/setter, allowing your downstream code to access the context with type checks. It's still possible to bypass the type checks by using the `.Get(key)` and `.Set(key, value)` methods so that you can work around possible inconveniences.

Context will dispose all objects it contains when it is disposed. Set the objects to `null` if you want to keep them alive after the context is disposed.

## Path Parser

The `Path.Parser` class provides a simple way to parse URL paths with a leading `/` with support for parameter bindings. 

Supported formats:

- `/path/to/resource`: exact match
- `/user/{userid}/posts/{postid}`, : parameter will match any string until the next `/` or the end of the path
- `/posts/{year}-{month}-{day}/`: parameter will match any string that is not a `/` in a non-greedy way (use with caution!)
- `/dir/{param?}`: allow 0 characters to be matched
- `/dir/{param: splat}`: parameter will match 1 or more characters all the way to the end of the string
- `/dir/{param?: splat}`: same as above but allow 0 character to be matched
- `/dir/{param: splat(N)}`: parameter will match exactly N segments separated by `/`
- `/dir/{param: /regex/}`: parameter will match the regex pattern (including any `/` if the regex permits)

Parameters will be written to `Context.UrlBindings`.

Parameters might come back as empty strings if 0 characters are matched. You should always check for it.

## Middleware

The `Middleware.MiddlewareStack<TContext>` class allow you to define function stacks. Functions higher in the stack can execute code before and after the next function.

Example:
```csharp
// Async-style middleware
async Task func1(TContext context, IAwaiter next)
{
	// func1_before
	await next;
	// func1_after
}

// Sync-style middleware
Task func2(TContext context, IAwaiter next)
{
	// func2_before
	next.GetAwaiter().GetResult();
	// func2_after
	return Middleware.CompletedTask;
}

async Task func3(TContext context, IAwaiter _) {
	// func3
	// You don't need to call next if you are the last function in the stack 
}

var stack = new MiddlewareStack<TContext>();
stack.Add(func1, func2);
stack.Add(func3);

await using var next = new StackingAwaiter();
await stack.Handle(context, next);
```

The execution order will be `func1_before`, `func2_before`, `func3`, `func2_after`, `func1_after`.

### Execution Internals

[`async`](https://github.com/dotnet/csharpstandard/blob/standard-v6/standard/classes.md#1415-async-functions)/[`await`](https://github.com/dotnet/csharpstandard/blob/standard-v6/standard/expressions.md#1188-await-expressions) syntactic sugar is only used to trick the compiler into transforming the method into a state machine. The middlewares are still executed in the same thread as the caller. If you need real multithreading, wrap the call inside a `Task.Run()`.

```csharp
await Task.Run(async () => {
    await Task.Yield(); // force the task to be continue in a thread pool
	await using var next = new StackingAwaiter();
	await stack.Handle(context, next);
}).ConfigureAwait(false);
```

### Exception Handling

If one Task throws an exception, the stack will stop executing, and the exception will be wrapped in an `AggregateException` and thrown to the `await next;` line of all the previous middlewares in the stack in order. Optionally, previous Task can catch the exception and handle it.

```csharp
async Task Recovery(TContext context, IAwaitable next) {
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
