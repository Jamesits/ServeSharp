using HttpMachine;
using IHttpMachine.Model;

namespace ServeSharp.HttpMachine.Example.Cli;

internal class HttpResponse : IHttpRequestResponse
{
    public MessageTypeKind MessageType { get; set; }
    public int StatusCode { get; set; } = 200;
    public string ResponseReason { get; set; } = "OK";
    public int MajorVersion { get; set; } = 1;
    public int MinorVersion { get; set; } = 1;
    public bool ShouldKeepAlive { get; set; } = false;
    public object UserContext { get; set; } = null;
    public string Method { get; set; } = "GET";
    public string RequestUri { get; set; } = "";
    public string Path { get; set; } = "";
    public string QueryString { get; set; } = "";
    public string Fragment { get; set; } = "";
    public MemoryStream Body { get; set; }
    public IDictionary<string, IEnumerable<string>> Headers { get; } = new Dictionary<string, IEnumerable<string>>();
    public bool IsTransferEncodingChunked { get; } = false;
    public int ChunkSize { get; } = 0;
    public bool IsEndOfMessage { get; } = true;
    public bool IsRequestTimedOut { get; } = false;
    public bool IsUnableToParseHttp { get; } = false;
    public string RemoteAddress { get; } = "";
    public int RemotePort { get; } = 0;
}
