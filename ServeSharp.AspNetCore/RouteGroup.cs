using ServeSharp.Core;
using ServeSharp.Core.Router;

namespace ServeSharp.AspNetCore;

public class RouteGroup(IPathGroup<Context, Route> parent, string path)
    : PassThroughRouteGroup<Context, Route>(parent, path);
