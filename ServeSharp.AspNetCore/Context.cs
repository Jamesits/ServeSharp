using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ServeSharp.AspNetCore;

public interface IHttp
{
    public HttpContext? HttpContext { get; set; }

    public Dictionary<string, string>? UrlBindings { get; set; }
}

public class Context : ServeSharp.Core.Context.Context
{
    public IHttp Http => As<IHttp>();
}
