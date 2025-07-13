namespace ServeSharp.Core;

/// <summary>
/// <a>IContext</a> represents a generic directory where all middleware functions can get and set anything.
/// </summary>
public interface IContext
{
    public T GetAdapter<T>();
}
