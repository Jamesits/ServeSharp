#nullable enable
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;

namespace ServeSharp.Core.Path
{
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
    }
}
