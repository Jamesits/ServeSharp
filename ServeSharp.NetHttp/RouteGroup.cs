using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using ServeSharp.Core.Middleware;
using ServeSharp.Core.Path;
using sly.parser.generator;

namespace ServeSharp.NetHttp
{
    public class RouteGroup : IPathGroup<Context, Route>
    {
        internal IPathGroup<Context, Route> _parent;
        internal string _path;

        public IPathGroup<Context, Route> Group(string path) => new RouteGroup()
        {
            _parent = _parent,
            _path = _path + path,
        };
        
        public Route Route(HttpMethod method, string path, HandleFunc<Context> handler) => _parent.Route(method, path, handler);
        public Route Get(string path, HandleFunc<Context> handler) => _parent.Get(path, handler);
        public Route Patch(string path, HandleFunc<Context> handler) => _parent.Patch(path, handler);
        public Route Post(string path, HandleFunc<Context> handler) => _parent.Post(path, handler);
        public Route Put(string path, HandleFunc<Context> handler) => _parent.Put(path, handler);
        public Route Delete(string path, HandleFunc<Context> handler) => _parent.Delete(path, handler);
        public Route Options(string path, HandleFunc<Context> handler) => _parent.Options(path, handler);
        public Route Head(string path, HandleFunc<Context> handler) => _parent.Head(path, handler);
        public Route Trace(string path, HandleFunc<Context> handler) => _parent.Trace(path, handler);
    }
}
