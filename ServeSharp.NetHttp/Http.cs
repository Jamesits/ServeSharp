#nullable enable
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ServeSharp.NetHttp.Test")]
namespace ServeSharp.NetHttp
{
    public interface IHttp
    {
        public HttpRequestMessage? Request { get; internal set; }

        public HttpResponseMessage? Response { get; internal set; }
    }
}