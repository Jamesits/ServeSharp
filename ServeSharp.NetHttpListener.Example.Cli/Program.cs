using System.Net;
using ServeSharp.NetHttpListener;
using ServeSharp.Core.Path;

var router = new Router();
router.Get("/", async (context, _) =>
{
    ArgumentNullException.ThrowIfNull(context, nameof(context));

    context.Response!.StatusCode = 200;
    context.Response!.ContentType = "text/html";
    await context.Response!.WriteStringAsync("<h1>It works!</h1>").ConfigureAwait(false);
});

using var listener = new HttpListener();
// We must set a prefix with a host name here. And you can't use 0.0.0.0 or [::]. This is ridiculous.
listener.Prefixes.Add("http://localhost:5000/");

Console.CancelKeyPress += (_, _) =>
{
    // This is the only way to break out from GetContextAsync(). This is ridiculous.
    listener.Abort();
};
listener.Start();
while (true)
{
    await router.ServeHttp(await listener.GetContextAsync());
}
