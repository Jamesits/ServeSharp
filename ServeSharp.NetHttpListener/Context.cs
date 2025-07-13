#nullable enable
using System;
using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ServeSharp.NetHttpListener;

public interface IHttp
{
    public HttpListenerContext HttpListenerContext { get; set; }
}

public class Context : ServeSharp.Core.Context.Context
{
    public IHttp Http => GetAdapter<IHttp>();

    public HttpListenerRequest? Request => Http.HttpListenerContext.Request;
    public HttpListenerResponse? Response => Http.HttpListenerContext.Response;
    public IPrincipal User => Http.HttpListenerContext.User;

    public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol) =>
        Http.HttpListenerContext.AcceptWebSocketAsync(subProtocol);
    public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, TimeSpan keepAliveInterval) =>
        Http.HttpListenerContext.AcceptWebSocketAsync(subProtocol, keepAliveInterval);
    public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval) =>
        Http.HttpListenerContext.AcceptWebSocketAsync(subProtocol, receiveBufferSize, keepAliveInterval);
}
