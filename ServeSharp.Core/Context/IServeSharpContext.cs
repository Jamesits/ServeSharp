#nullable enable
using System.Collections.Generic;

namespace ServeSharp.Core.Context;

public interface IServeSharpContext
{
    public Dictionary<string, string>? UrlBindings { get; set; }
}
