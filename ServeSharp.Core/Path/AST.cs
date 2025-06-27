#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ServeSharp.Core.Path;

/// <summary>
/// Base class for a simple LL(1) parser tree node.
/// </summary>
public abstract class Matcher
{
    public static string PathSeparator { get; set; } = "/";
    public virtual bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
    {
        throw new NotImplementedException();
    }
}

public class RootMatcher : Matcher
{
    private readonly List<Matcher> _matchers = [];

    public RootMatcher() { }

    public RootMatcher(params Matcher[] matchers) => _matchers.AddRange(matchers);

    public void Add(Matcher matcher) => _matchers.Add(matcher);

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var matcher in _matchers)
        {
            sb.Append($"{PathSeparator}{matcher}");
        }

        return sb.ToString();
    }

    // RootMatcher rules:
    // - All child matchers must succeed
    // - There must be nothing left after all child matchers have succeeded
    // - There must be a '/' between every child matcher
    public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
    {
        remainder = path;
        binding = null;

        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        foreach (var m in _matchers)
        {
            // nothing left to match, but there are matchers
            if (remainder.Length == 0)
            {
                return false;
            }

            // every child matcher must start and end with a "/"
            if (!remainder.StartsWith(PathSeparator, StringComparison.Ordinal))
            {
                return false;
            }

            remainder = remainder.Remove(0, 1);

            var r = m.Match(remainder, out var l, out var d);
            if (!r)
            {
                return false;
            }

            remainder = l;
            binding = binding.Merge(d);
        }

        // all child matcher must go through and there must be nothing left
        return (string.IsNullOrEmpty(remainder));
    }
}

/// <summary>
/// Class <c>AggregatedMatcher</c> is just a bunch of matchers concatenated together.
/// </summary>
public class AggregatedMatcher : Matcher
{
    private readonly List<Matcher> _matchers = [];

    public AggregatedMatcher() { }
    public AggregatedMatcher(params Matcher[] matchers)
    {
        Add(matchers);
    }

    public void Add(params Matcher[] matcher)
    {
        if (matcher == null) throw new ArgumentNullException(nameof(matcher));

        for (var i = 0; i < matcher.Length; i++)
        {
            // if BindingNonGreedyMatcher is followed by a StaticMatcher, it should stop when its remainder can be matched by the StaticMatcher.
            // This is to support non-greedy binding inside a segment, e.g. `/path/{year}-{month}-{day}.html`
            if ((i < matcher.Length - 1) && (matcher[i] is BindingNonGreedyMatcher bm) &&
                (matcher[i + 1] is StaticMatcher sm))
            {
                bm.Terminator = sm.Destination;
            }
            
            _matchers.Add(matcher[i]);
        }
    }

    public override string ToString() => $"{string.Join("", _matchers.Select(m => m.ToString()))}";

    // Every child matcher must succeed on a continuous path
    public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
    {
        remainder = path ?? throw new ArgumentNullException(nameof(path));
        binding = null;

        foreach (var matcher in _matchers)
        {
            var ret = matcher.Match(remainder, out var currentRemainder, out var d);
            if (!ret)
            {
                return false;
            }

            remainder = currentRemainder;
            binding = binding.Merge(d);
        }

        return true;
    }
}

/// <summary>
/// <c>StaticMatcher</c> matches any literal.
/// </summary>
public class StaticMatcher(string value) : Matcher
{
    internal string Destination { get; } = value;
    public override string ToString() => $"{Destination}";

    public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));

        binding = null;
        if (path.StartsWith(Destination, StringComparison.Ordinal))
        {
            remainder = path.Remove(0, Destination.Length);
            return true;
        }

        remainder = path;
        return false;
    }
}

/// <summary>
/// <c>BindingNonGreedyMatcher</c> matches anything before we hit the terminator string or the path separator for the first time.
/// </summary>
public class BindingNonGreedyMatcher(string dst, string? terminator = null) : Matcher
{
    internal string Destination { get; } = dst;
    internal string? Terminator { get; set; } = terminator;

    public override string ToString()
    {
        if (Terminator == null)
        {
            return $"{'{'}{Destination}: before(\"{PathSeparator}\"){'}'}";
        }
        else
        {
            return $"{'{'}{Destination}: before(either(\"{PathSeparator}\", \"{Terminator}\")){'}'}";
        }
    }

    public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));

        var cutPos = path.IndexOf(PathSeparator, StringComparison.Ordinal);
        if (Terminator != null)
        {
            var cutPos2 = path.IndexOf(Terminator, StringComparison.Ordinal);
            if (cutPos2 >= 0 && (cutPos < 0 || cutPos2 < cutPos))
            {
                cutPos = cutPos2;
            }
        }
        
        if (cutPos < 0)
        {
            // no terminator found, match the whole path
            binding = new Dictionary<string, string>
            {
                [Destination] = path,
            };
            remainder = "";
            return true;
        }

        remainder = path[cutPos..];
        binding = new Dictionary<string, string>
        {
            [Destination] = path[..cutPos],
        };
        return true;
    }
}

/// <summary>
/// <c>BindingSplatMatcher</c> matches N segments separated by the path separator. N=0 is a special case for matching anything (0 or more characters).
/// </summary>
public class BindingSplatMatcher(string destination, int repeat = 0) : Matcher
{
    internal string Destination { get; } = destination;
    internal int Repeat { get; } = repeat;

    public override string ToString() => $"{'{'}{Destination}: splat({(Repeat == 0 ? "anything" : Repeat)}){'}'}";

    public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));

        // special case: if _n == 0 then match anything (0 or more characters)
        if (Repeat == 0)
        {
            binding = new Dictionary<string, string>
            {
                [Destination] = path,
            };
            remainder = "";
            return true;
        }

        // match N segments
        var c = path.Split('/', Repeat + 1);

        // not getting N segments, return non-match
        if (c.Length < Repeat)
        {
            remainder = path;
            binding = null;
            return false;
        }

        remainder = c.Length == Repeat ? "" : "/" + c[Repeat];
        binding = new Dictionary<string, string>
        {
            [Destination] = c[..Repeat].Join("/"),
        };
        return true;
    }
}

/// <summary>
/// <c>BindingRegexMatcher</c> matches anything that matches the regular expression. It need not stop at the path separator, if the regular expression permits.
/// </summary>
public class BindingRegexMatcher(string dst, string re) : Matcher
{
    internal Regex Regex { get; } = new(re, RegexOptions.Compiled | RegexOptions.Singleline);

    public override string ToString() => $"{'{'}{dst}: {Regex.ToString() ?? "*any*"}{'}'}";

    public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));

        // regex set: match the first regex
        var m = Regex.Match(path);
        if (!m.Success)
        {
            remainder = path;
            binding = null;
            return false;
        }

        binding = new Dictionary<string, string>
        {
            [dst] = m.Value,
        };
        remainder = path.Remove(0, m.Value.Length);
        return true;
    }
}
