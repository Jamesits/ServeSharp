using System.Diagnostics;
using System.Net;
using ServeSharp.Core.Path;

namespace ServeSharp.NetHttp.Test;

public class ServerTest
{
    private Server _server;

    [SetUp]
    public void Setup()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        _server = new Server();
#pragma warning restore CS0618 // Type or member is obsolete
        _server.Router.Get("/", async (context, _) =>
        {
            Debug.Assert(context.Http.Response != null, "context.Http.Response != null");
            context.Http.Response.StatusCode = HttpStatusCode.OK;
            context.Http.Response.Content = new StringContent("<h1>It works!</h1>");
            context.Http.Response.Content.Headers.ContentType.MediaType = "text/html";
        });
    }

    [TearDown]
    public void TearDown()
    {
        _server.Dispose();
    }

    [Test]
    public async Task Test1()
    {
        // await _server.ListenAndServe();
    }
}
