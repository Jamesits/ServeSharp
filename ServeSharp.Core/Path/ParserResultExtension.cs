using System;
using System.IO;
using System.Linq;
using sly.parser;

namespace ServeSharp.Core.Path
{
    public static class ParserResultExtension
    {
        public static void ThrowIfError<T1, T2>(this ParseResult<T1, T2> result)
            where T1 : struct, Enum // T1 must be non-nullable and an Enum
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            if (result.IsError)
            {
                throw new AggregateException(result.Errors.Select(e => new InvalidDataException($"{e.Line}:{e.Column}: [{e.ErrorType}] {e.ErrorMessage}")));
            }
        }
    }
}