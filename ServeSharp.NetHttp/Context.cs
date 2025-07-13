using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ServeSharp.NetHttp.Test")]
namespace ServeSharp.NetHttp;

public interface IHttp
{
    public HttpRequestMessage? Request { get; set; }

    public HttpResponseMessage? Response { get; set; }
}

public class Context : ServeSharp.Core.Context.Context
{
    public IHttp Http => GetAdapter<IHttp>();

    public HttpRequestMessage? Request
    {
        get => Http.Request;
        set => Http.Request = value;
    }

    public HttpResponseMessage? Response
    {
        get => Http.Response;
        set => Http.Response = value;
    }
}
