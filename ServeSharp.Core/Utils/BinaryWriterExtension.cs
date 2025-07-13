using System.IO;
using System.Text;

namespace ServeSharp.Core.Utils;

public static class BinaryWriterExtension
{
    public static void WriteString(this BinaryWriter b, string s)
    {
        b!.Write(Encoding.UTF8.GetBytes(s));
    }
}