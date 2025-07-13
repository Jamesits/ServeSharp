using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpMachine;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using ServeSharp.HttpMachine;
using ServeSharp.HttpMachine.Example.Cli;

var router = new Router();
router.Get("/", (context, _) =>
{
    var resp = new HttpResponse();
    resp.Headers.Add("Content-Type", ["text/html"]);
    resp.Body = new MemoryStream(Encoding.UTF8.GetBytes("<h1>It works!</h1>"));

    context.Http.Response = resp;
    return Middleware.CompletedTask;
});

var ipAddress = new IPAddress([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
var ipEndPoint = new IPEndPoint(ipAddress, 5000);
using var server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
server.Bind(ipEndPoint);

while (true)
{
    var receivingSocket = await server.AcceptAsync();
    // emit a task to handle the TCP connection
    _ = Task.Run(async () =>
    {
        // Keep receiving until exactly one HTTP request has been received
        using var handler = new HttpParserDelegate();
        using var parser = new HttpCombinedParser(handler);
        while (true)
        {
            var buffer = new byte[1024];
            await receivingSocket.ReceiveAsync(buffer, SocketFlags.None);
            parser.Execute(buffer);

            if (handler.HttpRequestResponse.IsUnableToParseHttp) throw new InvalidDataException("Unable to parse HTTP request");
            if (handler.HttpRequestResponse.IsRequestTimedOut)
                throw new TimeoutException("HTTP request receive timed out");

            if (handler.HttpRequestResponse.IsEndOfMessage) break;
        }

        // Routing
        using var ctx = new Context();
        ctx.Http.Request = handler.HttpRequestResponse;
        await router.ServeHttp(ctx);

        // Replying
        await receivingSocket.SendAsync(ctx.Response!.ToBytes());
        receivingSocket.Close();
    }).ConfigureAwait(false);
}
