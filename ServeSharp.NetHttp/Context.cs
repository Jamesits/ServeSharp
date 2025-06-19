#nullable enable
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ServeSharp.NetHttp.Test")]
namespace ServeSharp.NetHttp
{
    public class Context : EventArgs, IDisposable
    {
        public HttpRequestMessage? Request { get; internal set; }

        public HttpResponseMessage? Response { get; internal set; }

        public void Dispose()
        {
            Request?.Dispose();
            Response?.Dispose();
        }
    }
}