using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServeSharp.NetHttp
{
    public static class BinaryWriterExtension
    {
        public static void WriteString(this BinaryWriter b, string s)
        {
            b?.Write(Encoding.UTF8.GetBytes(s));
        }
    }

    [Obsolete("For testing only, do not use in production")]
    public class Server
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Socket socket;

        public Router Router { get; } = new Router();

        public async Task ListenAndServe()
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var ipEndpoint = new IPEndPoint(IPAddress.Any, 8080);
            socket.Bind(ipEndpoint);
            socket.Listen(1024);

            while (true)
            {
                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                var conn = await socket.AcceptAsync();
                var buffer = new byte[1024];
                var received = await conn.ReceiveAsync(buffer, SocketFlags.None);
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

                using var m = new MemoryStream();
                await using var writer = new BinaryWriter(m, Encoding.UTF8);
                writer.WriteString($"HTTP/1.1 {(int)(context.Http.Response.StatusCode)} {context.Http.Response.ReasonPhrase}\r\n");
                // var str = System.Text.Encoding.Default.GetString(m.ToArray());

                foreach (var h in context.Http.Response.Headers)
                {
                    foreach (var value in h.Value)
                    {
                        writer.WriteString($"{h.Key}: {value}\r\n");
                    }
                }
                foreach (var h in context.Http.Response.Content.Headers)
                {
                    foreach (var value in h.Value)
                    {
                        writer.WriteString($"{h.Key}: {value}\r\n");
                    }
                }
                foreach (var h in context.Http.Response.TrailingHeaders)
                {
                    foreach (var value in h.Value)
                    {
                        writer.WriteString($"{h.Key}: {value}\r\n");
                    }
                }
                writer.WriteString("\r\n");

                if (context.Http.Response.Content != null)
                {
                    var buf = await context.Http.Response.Content.ReadAsByteArrayAsync();
                    writer.Write(buf);
                    writer.WriteString("\r\n\r\n");
                }
                writer.Flush();
                await m.FlushAsync();
                
                await conn.SendAsync(m.ToArray(), SocketFlags.None);
                conn.Close();
            }
        }
    }
}
