#nullable enable
using IHttpMachine.Model;

namespace ServeSharp.HttpMachine;

public interface IHttp
{
    public IHttpRequestResponse? Request { get; set; }
    public IHttpRequestResponse? Response { get; set; }
}

public class Context : ServeSharp.Core.Context.Context
{
    public IHttp Http => GetAdapter<IHttp>();

    public IHttpRequestResponse? Request => Http.Request;
    public IHttpRequestResponse? Response => Http.Response;
}
