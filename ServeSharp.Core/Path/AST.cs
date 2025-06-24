#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ServeSharp.Core.Path
{
    /// <summary>
    /// Base class for a simple LL(1) parser tree node.
    /// </summary>
    public abstract class Matcher
    {
        public virtual bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
        {
            throw new NotImplementedException();
        }
    }

    public class RootMatcher : Matcher
    {
        private readonly List<Matcher> _matchers = new List<Matcher>();

        public RootMatcher() { }

        public RootMatcher(params Matcher[] matchers) => _matchers.AddRange(matchers);
        
        public void Add(Matcher matcher) => _matchers.Add(matcher);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[AST]");
            foreach (var matcher in _matchers)
            {
                sb.AppendLine($"  /{matcher}");
            }

            sb.AppendLine("");
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

            if (path == "")
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
                if (remainder[0] != '/')
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
            return (remainder == "");
        }
    }

    public class AggregatedMatcher : Matcher
    {
        private readonly List<Matcher> _matchers = new List<Matcher>();

        public AggregatedMatcher() {}
        public AggregatedMatcher(params Matcher []matchers)
        {
            _matchers.AddRange(matchers);
        }

        public void Add(Matcher matcher) => _matchers.Add(matcher);

        public override string ToString() => $"[Segment] {string.Join("", _matchers.Select(m => m.ToString()))}";

        // Every child matcher must succeed on a continuous path
        public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
        {
            binding = null;
            remainder = path;
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
    public class StaticMatcher : Matcher
    {
        private readonly string _value;
        public StaticMatcher(string value)
        {
            _value = value;
        }

        public override string ToString() => $"{_value}";

        public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
        {
            binding = null;
            if (path.StartsWith(_value))
            {
                remainder = path.Remove(0, _value.Length);
                return true;
            }

            remainder = path;
            return false;
        }
    }

    /// <summary>
    /// <c>BindingSplatMatcher</c> matches N segments separated by '/'. N=0 is a special case for matching anything (0 or more characters).
    /// </summary>
    public class BindingSplatMatcher : Matcher
    {
        private readonly string _bindingDestination;
        private readonly int _n;

        public BindingSplatMatcher(string dst, int n = 0)
        {
            _bindingDestination = dst;
            _n = n;
        }

        public override string ToString() => $"[{_bindingDestination} -> splat({(_n == 0 ? "anything" : _n.ToString())})]";

        public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
        {
            // special case: if _n == 0 then match anything (0 or more characters)
            if (_n == 0)
            {
                binding = new Dictionary<string, string>
                {
                    [_bindingDestination] = path,
                };
                remainder = "";
                return true;
            }

            // match N segments
            var c = path.Split('/', _n + 1);

            // not getting N segments, return non-match
            if (c.Length < _n)
            {
                remainder = path;
                binding = null;
                return false;
            }

            remainder = c.Length == _n ? "" : "/" + c[_n];
            binding = new Dictionary<string, string>
            {
                [_bindingDestination] = c[.._n].Join("/"),
            };
            return true;
        }
    }

    /// <summary>
    /// <c>BindingRegexMatcher</c> matches anything that matches the regular expression. It does not stop at '/'.
    /// </summary>
    public class BindingRegexMatcher : Matcher
    {
        private readonly string _bindingDestination;
        private readonly Regex _regex;

        public BindingRegexMatcher(string dst, string regex)
        {
            _bindingDestination = dst;
            _regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public override string ToString() => $"[{_bindingDestination} -> {_regex.ToString() ?? "*any*"}]";

        public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
        {
            // regex set: match the first regex
            var m = _regex.Match(path);
            if (!m.Success)
            {
                remainder = path;
                binding = null;
                return false;
            }

            binding = new Dictionary<string, string>
            {
                [_bindingDestination] = m.Value,
            };
            remainder = path.Remove(0, m.Value.Length);
            return true;
        }
    }
}
