#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ServeSharp.NetHttp.Test")]
namespace ServeSharp.NetHttp
{
    public interface IHttp
    {
        public HttpRequestMessage? Request { get; set; }

        public HttpResponseMessage? Response { get; set; }

        public Dictionary<string, string>? UrlBindings { get; internal set; }
    }

    public class Context : ServeSharp.Core.Context.Context
    {
        public IHttp Http => Get<IHttp>();
    }
}
