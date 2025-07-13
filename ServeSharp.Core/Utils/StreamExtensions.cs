#nullable enable
using System.IO;

namespace ServeSharp.Core.Utils;

public static class StreamExtensions
{
    public static byte[] ToByteArray(this Stream? stream)
    {
        if (stream == null) return [];

        if (stream is MemoryStream ms)
            return ms.ToArray();

        using var outStream = new MemoryStream();
        stream.CopyTo(outStream);
        return outStream.ToArray();
    }
}
