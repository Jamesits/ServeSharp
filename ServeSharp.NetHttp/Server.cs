using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServeSharp.NetHttp
{
    [Obsolete("For testing only, do not use in production")]
    public class Server : IDisposable
    {
        private bool _disposed;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public Router Router { get; } = new Router();

        public async Task ListenAndServe()
        {
            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var ipEndpoint = new IPEndPoint(IPAddress.Any, 8080);
            socket.Bind(ipEndpoint);
            socket.Listen(1024);

            while (true)
            {
                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                var conn = await socket.AcceptAsync().ConfigureAwait(false);
                var buffer = new byte[1024];
                var received = await conn.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);
                var s = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine(s);

                var lines = s.Split("\r\n");
                var httpProtocolInfo = lines[0].Split(" ");

                // parse HTTP
                using var httpRequest = new HttpRequestMessage();
                httpRequest.Content = null;
                httpRequest.Method = new HttpMethod(httpProtocolInfo[0]);
                httpRequest.RequestUri = new Uri($"http://example.com{httpProtocolInfo[1]}");
                httpRequest.Version = Version.Parse(httpProtocolInfo[2].Split('/').Last());

                foreach (var l in lines.Skip(1))
                {
                    var header = l.Split(": ");
                    if (header.Length < 2) break;
                    httpRequest.Headers.TryAddWithoutValidation(header[0], header[1]);
                }

                // reply
                using var context = new Context();
                context.Http.Request = httpRequest;
                context.Http.Response = new HttpResponseMessage();
                await Router.Handle(context);

                await conn.SendAsync(await context.Http.Response.ToByteArray().ConfigureAwait(false), SocketFlags.None).ConfigureAwait(false);
                conn.Close();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cts.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
