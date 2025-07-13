using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ServeSharp.AspNetCore;
using ServeSharp.Core.Path;

var router = new Router();
router.Get("/", async (context, _) =>
{
    ArgumentNullException.ThrowIfNull(context, nameof(context));

    context.Http.HttpContext!.Response.StatusCode = StatusCodes.Status200OK;
    context.Http.HttpContext!.Response.ContentType = "text/html";
    await context.Http.HttpContext.Response.WriteAsync("<h1>It works!</h1>").ConfigureAwait(false);
});

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#wildcard-and-catch-all-routes
app.Map("/{*_}", router.ServeHttp);

Console.CancelKeyPress += async (_, _) =>
{
    await app.StopAsync();
};
await app.RunAsync();
