using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ServeSharp.Core;
using ServeSharp.Core.Middleware;

namespace ServeSharp.NetHttp;

public class RouteGroup : IPathGroup<Context, Route>
{
    private readonly IPathGroup<Context, Route> _parent;
    private readonly string _path;
    private readonly List<HandleFunc<Context>> _middlewares = new ();

    public RouteGroup(IPathGroup<Context, Route> parent, string path)
    {
        _parent = parent;
        _path = path;

        // read from parent
        AutoHead = _parent.AutoHead;
    }

    public bool AutoHead { get; set; }

    public void Use(params HandleFunc<Context>[] handlers)
    {
        _middlewares.AddRange(handlers);
    }

    public IPathGroup<Context, Route> Group(string path) => new RouteGroup(_parent, _path + path);

    public Route Handle(HttpMethod? method, string path, params HandleFunc<Context>[] handlers) => _parent.Handle(method, _path + path, _middlewares.Concat(handlers).ToArray());
}