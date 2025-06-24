#nullable enable
using System;
using System.Linq;

namespace ServeSharp.Core.Middleware
{
    public static class AggregateExceptionExtension
    {
        public static AggregateException? Append(this AggregateException? a, Exception? ex)
        {
            if (ex == null) return a;
            a ??= new AggregateException();

            return new AggregateException(a.InnerExceptions.Append(ex));
        }
    }
}