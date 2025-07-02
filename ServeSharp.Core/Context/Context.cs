#nullable enable
using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;

namespace ServeSharp.Core.Context;

public class Context : IDisposable
{
    private bool _disposed;
    private readonly Dictionary<string, object> _dict = new Dictionary<string, object>();
    private readonly DictionaryAdapterFactory _factory = new DictionaryAdapterFactory();

    public object this[string key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    public void Set(string key, object thing)
    {
        _dict.Add(key, thing);
    }

    public object Get(string key)
    {
        return _dict[key];
    }

    public object GetOrDefault(string key, object defaultValue)
    {
        return _dict.GetValueOrDefault(key, defaultValue);
    }

    public bool TryGetValue(string key, out object value)
    {
        return _dict.TryGetValue(key, out value);
    }

    public void Remove(string key)
    {
        _dict.Remove(key);
    }

    public T As<T>()
    {
        return _factory.GetAdapter<T>(_dict);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        lock (this)
        {
            if (_disposed) return;

            // free managed resources
            if (disposing)
            {
                foreach (var value in _dict.Values)
                {
                    if (value is IAsyncDisposable vad)
                    {
                        var task = vad.DisposeAsync();
                        if (!task.IsCompleted)
                        {
                            task.AsTask().GetAwaiter().GetResult();
                        }
                    }
                    if (value is IDisposable vd)
                    {
                        vd.Dispose();
                    }
                }
            }

            _disposed = true;
        }
    }

    ~Context()
    {
        Dispose(false);
    }
}
