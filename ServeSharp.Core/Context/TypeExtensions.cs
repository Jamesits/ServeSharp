#nullable enable
using System;
using System.Reflection;

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