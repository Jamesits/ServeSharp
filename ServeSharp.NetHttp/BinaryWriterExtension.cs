using System.IO;
using System.Text;

namespace ServeSharp.NetHttp;

public static class BinaryWriterExtension
{
    public static void WriteString(this BinaryWriter b, string s)
    {
        b?.Write(Encoding.UTF8.GetBytes(s));
    }
}