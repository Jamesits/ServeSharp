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
}

public class Context : ServeSharp.Core.Context.Context
{
    public IHttp Http => GetAdapter<IHttp>();

    public IBrowser Browser
    {
        get => Http.Browser;
        set => Http.Browser = value;
    }
    public IFrame Frame
    {
        get => Http.Frame;
        set => Http.Frame = value;
    }

    public string SchemeName
    {
        get => Http.SchemeName;
        set => Http.SchemeName = value;
    }
    public IRequest Request
    {
        get => Http.Request;
        set => Http.Request = value;
    }
}
