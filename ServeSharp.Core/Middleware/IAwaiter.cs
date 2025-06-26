using System.Runtime.CompilerServices;

namespace ServeSharp.Core.Middleware;

/// <summary>
/// Interface for <c>await something</c> that returns nothing.
/// </summary>
public interface IAwaiter : INotifyCompletion
{
    public bool IsCompleted { get; }
    public void GetResult();
}