using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServeSharp.HttpMachine;
public static class HttpListenerResponseExtensions
{
    public static async ValueTask WriteStringAsync(this HttpListenerResponse response, string content, CancellationToken cancellationToken = default)
    {
        var buf = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buf.LongLength;
        await response.OutputStream.WriteAsync(buf, cancellationToken);
        response.OutputStream.Close();
    }
}