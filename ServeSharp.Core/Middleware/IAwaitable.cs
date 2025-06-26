namespace ServeSharp.Core.Middleware;

/// <summary>
/// Explicit interface to support the compiling of async/await.
/// </summary>
public interface IAwaitable
{
    public IAwaiter GetAwaiter();
}