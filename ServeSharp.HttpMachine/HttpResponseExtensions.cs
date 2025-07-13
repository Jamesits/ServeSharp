using System.IO;
using IHttpMachine.Model;
using ServeSharp.Core.Utils;

namespace ServeSharp.HttpMachine;

public static class HttpResponseExtensions
{
    public static byte[] ToBytes(this IHttpRequestResponse response)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        // write header
        writer.WriteString($"HTTP/{response.MajorVersion}.{response.MinorVersion} {response.StatusCode} {response.ResponseReason}\r\n");
        foreach (var header in response.Headers)
        {
            foreach (var entry in header.Value)
            {
                writer.WriteString($"{header.Key}: {entry}\r\n");
            }
        }
        writer.WriteString("\r\n");

        // write body
        writer.Write(response.Body.ToArray());
        return stream.ToArray();
    }
}
