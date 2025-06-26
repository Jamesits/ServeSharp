// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
#pragma warning disable CA1707
using sly.lexer;

namespace ServeSharp.Core.Path;

public enum RouteToken
{
    [Sugar("/")]
    ROOT,

    [CustomId("-_0-9a-zA-Z.~!$&'()*+,;=:%", "-_0-9a-zA-Z.~!$&'()*+,;=:%")]
    PCHARS,

    [Sugar("{")]
    [Push("bind")]
    BIND_START,

    [Sugar("}")]
    [Mode("bind")]
    [Pop]
    BIND_END,

    [Sugar(":")]
    [Mode("bind")]
    BIND_SEP,

    [AlphaNumDashId]
    [Mode("bind")]
    BIND_DST,

    [String("/", "\\")]
    [Mode("bind")]
    BIND_REGEXP,

    [Keyword("splat")]
    [Mode("bind")]
    BIND_SPLAT,

    [Sugar("(")]
    [Mode("bind")]
    BRACKET_L,

    [Int]
    [Mode("bind")]
    BIND_SPLAT_COUNT,

    [Sugar(")")]
    [Mode("bind")]
    BRACKET_R,
}