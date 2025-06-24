using System.Text;

namespace ServeSharp.Core.Path
{
    internal static class StringArrayExtension
    {
        /// <summary>
        /// Join a string array into one string. <c>string.Join</c> ignores empty elements, but we don't.
        /// </summary>
        /// <param name="src">Source string array</param>
        /// <param name="sep">Separator</param>
        /// <returns>The joined string</returns>
        public static string Join(this string[] src, string sep)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < src.Length - 1; ++i)
            {
                sb.Append(src[i]);
                sb.Append(sep);
            }

            sb.Append(src[^1]);
            return sb.ToString();
        }
    }
}
