#nullable enable
using System.Collections.Generic;
using System.Linq;

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
    }
}
