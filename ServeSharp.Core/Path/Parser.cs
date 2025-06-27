// ReSharper disable UnusedMember.Global
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
namespace ServeSharp.Core.Path;

[ParserRoot("route")]
public sealed class Parser
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
    public static Matcher Route(List<Group<RouteToken, Matcher>> segments)
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

    [Production("segment : [ literal | binding_segment | binding_splat_any | binding_splat | binding_regex ]*")]
    public static Matcher Segment(List<Matcher> m) => m.Count == 1 ? m[0] : new AggregatedMatcher(m.ToArray());

    // ReSharper disable once StringLiteralTypo
    // Matches string literal
    [Production("literal: PCHARS+")]
    public static Matcher Literal(List<Token<RouteToken>> pcs) => new StaticMatcher(string.Concat(pcs.Select(x => x.StringWithoutQuotes)));

    // "{name}" - matches everything before next literal
    [Production("binding_segment : BIND_START [d] BIND_DST BIND_END [d]")]
    public static Matcher BindingSegment(Token<RouteToken> bindDst) => new BindingNonGreedyMatcher(bindDst.StringWithoutQuotes);

    // "{name : splat}" - matches 0 or more characters (use at the end only)
    [Production("binding_splat_any : BIND_START [d] BIND_DST BIND_SEP [d] BIND_SPLAT [d] BIND_END [d]")]
    public static Matcher BindingSplatCount(Token<RouteToken> bindDst) => new BindingSplatMatcher(bindDst.StringWithoutQuotes, 0);

    // "{name : splat(N)}" - matches N segments separated by '/'
    [Production("binding_splat : BIND_START [d] BIND_DST BIND_SEP [d] BIND_SPLAT [d] BRACKET_L [d] BIND_SPLAT_COUNT BRACKET_R [d] BIND_END [d]")]
    public static Matcher BindingSplatCount(Token<RouteToken> bindDst, Token<RouteToken> splatCount) => new BindingSplatMatcher(bindDst.StringWithoutQuotes, splatCount.IntValue);

    // "{name : /regex/}"
    [Production("binding_regex : BIND_START [d] BIND_DST BIND_SEP [d] BIND_REGEXP BIND_END [d]")]
    public static Matcher BindingRegex(Token<RouteToken> bindDst, Token<RouteToken> r) => new BindingRegexMatcher(bindDst.StringWithoutQuotes, r.StringWithoutQuotes);
}
