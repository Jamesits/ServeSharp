#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ServeSharp.Core.Path
{
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

        public RootMatcher()
        {
        }

        public RootMatcher(params Matcher[] matchers)
        {
            _matchers.AddRange(matchers);
        }

        public void Add(Matcher matcher) => _matchers.Add(matcher);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[AST]");
            foreach (var matcher in _matchers)
            {
                sb.AppendLine($"  {matcher}");
            }

            sb.AppendLine("");
            return sb.ToString();
        }

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

    public class BindingMatcher : Matcher
    {
        private readonly string _bindingDestination;
        private readonly Regex? _regex;

        public BindingMatcher(string dst, string? regex = null)
        {
            _bindingDestination = dst;
            if (regex != null)
            {
                _regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        public override string ToString() => $"[{_bindingDestination} -> {_regex?.ToString() ?? "*any*"}]";

        public override bool Match(string path, out string remainder, out Dictionary<string, string>? binding)
        {
            // regex unset; match ends before the first "/"
            if (_regex == null)
            {
                var cutPtr = path.IndexOf('/', StringComparison.Ordinal);
                if (cutPtr == -1)
                {
                    remainder = "";
                }
                else
                {
                    remainder = path[cutPtr..];
                    path = path.Remove(cutPtr);
                }

                binding = new Dictionary<string, string>
                {
                    [_bindingDestination] = path,
                };
                return true;
            }

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
