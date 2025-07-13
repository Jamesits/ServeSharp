using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ServeSharp.Core.Utils;

namespace ServeSharp.NetHttp;

public static class HttpResponseMessageExtensions
{
    public static async Task<byte[]> ToByteArray(this HttpResponseMessage response)
    {
        if (response == null) throw new InvalidOperationException("null response");

        using var m = new MemoryStream();
        var writer = new BinaryWriter(m, Encoding.UTF8);
        await using var writer1 = writer.ConfigureAwait(false);
        writer.WriteString($"HTTP/1.1 {(int)response.StatusCode} {response.ReasonPhrase}\r\n");
        // var str = System.Text.Encoding.Default.GetString(m.ToArray());

        foreach (var h in response.Headers)
        {
            foreach (var value in h.Value)
            {
                writer.WriteString($"{h.Key}: {value}\r\n");
            }
        }
        foreach (var h in response.Content.Headers)
        {
            foreach (var value in h.Value)
            {
                writer.WriteString($"{h.Key}: {value}\r\n");
            }
        }
        foreach (var h in response.TrailingHeaders)
        {
            foreach (var value in h.Value)
            {
                writer.WriteString($"{h.Key}: {value}\r\n");
            }
        }
        writer.WriteString("\r\n");

        if (response.Content != null)
        {
            var buf = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            writer.Write(buf);
            writer.WriteString("\r\n\r\n");
        }
        writer.Flush();
        await m.FlushAsync().ConfigureAwait(false);

        return m.ToArray();
    }
}