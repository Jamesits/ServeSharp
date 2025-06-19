using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.parser;

[assembly: InternalsVisibleTo("ServeSharp.Core.Test")]
namespace ServeSharp.Core.Route
{
    [ParserRoot("route")]
    internal class Parser
    {
        public static Parser<RouteToken, Matcher> New()
        {
            var i = new Parser();
            var b = new ParserBuilder<RouteToken, Matcher>();
            var r = b.BuildParser(i, ParserType.EBNF_LL_RECURSIVE_DESCENT);

#if DEBUG
            // print warnings
            if (r.Errors.Count > 0)
            {
                foreach (var e in r.Errors)
                {
                    Console.WriteLine($"[{e.Level}] {e.Code}: {e.Message}");
                }
            }
#endif

            if (r.IsError)
            {
                throw new AggregateException(r.Errors.Select(e => new InvalidDataException($"[{e.Level}] {e.Code}: {e.Message}")));
            }

            return r.Result;
        }

        [Production("route : ( ROOT [d] segment )*")]
        public Matcher Route(List<Group<RouteToken, Matcher>> segments)
        {
            var ret = new RootMatcher();
            foreach (var segment in segments)
            {
                for (var i = 0; i < segment.Count; i++)
                {
                    ret.Add(segment.Value(i));
                }
            }

            return ret;
        }

        [Production("segment : [ literal | binding ]*")]
        public Matcher Segment(List<Matcher> m) =>  m.Count == 1 ? m[0] : new AggregatedMatcher(m.ToArray());

        // ReSharper disable once StringLiteralTypo
        [Production("literal: PCHARS+")]
        public Matcher Literal(List<Token<RouteToken>> pcs)
        {
            return new StaticMatcher(string.Concat(pcs.Select(x => x.StringWithoutQuotes)));
        }

        [Production("binding : BIND_START[d] BIND_DST (BIND_SEP [d] BIND_REGEXP)? BIND_END[d]")]
        public Matcher Binding(Token<RouteToken> bindDst, ValueOption<Group<RouteToken, Matcher>> regexp)
        {
            var re = regexp.Match(group => group.Token(0).StringWithoutQuotes, () => null);
            return new BindingMatcher(bindDst.StringWithoutQuotes, re);
        }
    }
}
