using System;

namespace ServeSharp.Core.Utils;

public class GenericProperty<T>
{
    public T Value { get; set; }

    public static implicit operator T(GenericProperty<T> value)
    {
        return value.Value;
    }

    public static implicit operator GenericProperty<T>(T value)
    {
        return new GenericProperty<T> { Value = value };
    }

    public T ToT() => Value;

    public GenericProperty<T> ToGenericProperty() => this;
}
