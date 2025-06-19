#nullable enable
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ServeSharp.Core.Test")]
namespace ServeSharp.Core.Middleware
{
    public class Context: EventArgs, IDisposable
    {
        public HttpRequestMessage? Request { get; internal set; }

        public HttpResponseMessage? Response { get; internal set; }

        public ExceptionHandleFunc? ExceptionHandler { get; set; }

        public void Dispose()
        {
            Request?.Dispose();
            Response?.Dispose();
        }
    }
}