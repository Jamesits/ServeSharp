#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServeSharp.Core.Path;

internal static class DictionaryExtension
{
    public static Dictionary<TKey, TValue>? Merge<TKey, TValue>(this Dictionary<TKey, TValue>? dst, Dictionary<TKey, TValue>? src)
    {
        if (dst == null)
        {
            return src;
        }

        if (src == null)
        {
            return dst;
        }

        src.ToList().ForEach(item => dst.Add(item.Key, item.Value));
        return dst;
    }

    public static bool Equal<TKey, TValue>(this Dictionary<TKey, TValue>? current, Dictionary<TKey, TValue>? other)
    {
        if (current == null && other == null)
        {
            return true;
        }

        if (current == null || other == null)
        {
            return false;
        }

        return current.OrderBy(x => x.Key).SequenceEqual(other.OrderBy(x => x.Key));
    }

    public static string String<TKey, TValue>(this Dictionary<TKey, TValue>? current)
    {
        if (current == null)
        {
            return "[null]";
        }

        var sb = new StringBuilder();
        sb.Append($"[{current.GetType()}]");
        sb.AppendLine(" {");
        foreach (var kv in current)
        {
            sb.AppendLine($"  \"{kv.Key}\": \"{kv.Value}\"");
        }

        sb.Append('}');
        return sb.ToString();
    }
}