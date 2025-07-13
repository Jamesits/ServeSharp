#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Castle.Components.DictionaryAdapter;

namespace ServeSharp.Core.Context;

public static class TypeExtensions
{
    public static object? GetDefault(this Type type)
    {
        if (type.GetTypeInfo().IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }
}

public class Context : IDisposable
{
    private bool _disposed;
    private readonly Dictionary<string, object> _dict = new();
    private readonly DictionaryAdapterFactory _factory = new();

    public object this[string key]
    {
        get => Get(key);
        set => BlindSet(key, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BlindSet(string key, object thing)
    {
        _dict.Add(key, thing);
    }

    public object? Set(string key, object thing)
    {
        var originalObject = GetOrDefault(key, null);
        BlindSet(key, thing);
        return originalObject;
    }

    public T? Set<T>(string key, T thing) where T : class
    {
        var originalObject = GetOrDefault(key, null);
        BlindSet(key, thing);
        return originalObject as T;
    }

    public object Get(string key)
    {
        return _dict[key];
    }

    public T Get<T>(string key) where T : class
    {
        return Get(key) as T ?? throw new NullReferenceException();
    }

    public object? GetOrDefault(string key, object? defaultValue)
    {
        return _dict!.GetValueOrDefault(key, defaultValue);
    }

    public T? GetOrDefault<T>(string key, T? defaultValue) where T : class
    {
        return GetOrDefault(key, (object?)defaultValue) as T;
    }

    public bool TryGetValue(string key, out object value)
    {
        return _dict.TryGetValue(key, out value);
    }

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
