#nullable enable
using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;

namespace ServeSharp.Core.Context;

public class Context : Dictionary<string, object>, IDisposable, IContext
{
    private bool _disposed;
    private readonly DictionaryAdapterFactory _factory = new();

    public object? Swap(string key, object thing)
    {
        var originalObject = GetOrDefault(key, null);
        Add(key, thing);
        return originalObject;
    }

    public T? Swap<T>(string key, T thing) where T : class
    {
        var originalObject = GetOrDefault(key, default(T));
        Add(key, thing);
        return originalObject as T;
    }

    public object Get(string key) => this[key];

    public T Get<T>(string key) where T : class => Get(key) as T ?? throw new KeyNotFoundException();

    public object? GetOrDefault(string key, object? defaultValue) => this!.GetValueOrDefault(key, defaultValue);

    public T? GetOrDefault<T>(string key, T? defaultValue) where T : class => GetOrDefault(key, (object?)defaultValue) as T;

    public bool TryGetValue<T>(string key, out T? value) where T : class
    {
        var ret = TryGetValue(key, out var realValue);
        if (realValue is not T tValue)
        {
            value = typeof(T).GetDefault() as T;
            return false;
        }

        value = tValue;
        return true;
    }

    public T GetAdapter<T>() => _factory.GetAdapter<T>(this);

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
                foreach (var value in Values)
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
