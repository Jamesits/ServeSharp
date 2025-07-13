#nullable enable
using System;
using ServeSharp.Core.Middleware;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ServeSharp.Core.Router;

public abstract class PassThroughRouteGroup<TContext, TRoute> : IPathGroup<TContext, TRoute>
{
    private readonly IPathGroup<TContext, TRoute> _parent;
    private readonly string _path;
    private readonly List<HandleFunc<TContext>> _middlewares = [];

    protected PassThroughRouteGroup(IPathGroup<TContext, TRoute> parent, string path)
    {
        _parent = parent;
        _path = path;

        // read from parent
        AutoHead = _parent.AutoHead;
    }

    public bool AutoHead { get; set; }

    public void Use(params HandleFunc<TContext>[] handlers)
    {
        _middlewares.AddRange(handlers);
    }

    public IPathGroup<TContext, TRoute> Group(string path) => (IPathGroup<TContext, TRoute>)Activator.CreateInstance(GetType(), _parent, _path + path);

    public TRoute Handle(HttpMethod? method, string path, params HandleFunc<TContext>[] handlers) => _parent.Handle(method, _path + path, _middlewares.Concat(handlers).ToArray());
}
