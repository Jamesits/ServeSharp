using System.Collections.Generic;
using CefSharp;

namespace ServeSharp.CefSharpCore;

public interface IHttp
{
    // request
    public IBrowser Browser { get; set; }
    public IFrame Frame { get; set; }
    public string SchemeName { get; set; }
    public IRequest Request { get; set; }

    // response
    public IResourceHandler? ResourceHandler { get; set; }

    public Dictionary<string, string>? UrlBindings { get; set; }
}

public class Context : ServeSharp.Core.Context.Context
{
    public IHttp Http => Get<IHttp>();
}
