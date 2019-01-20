using System;
using System.Collections.Generic;
using System.Text;
using Desh.Parsing;

namespace Desh.Test
{
    public class ParseLogger : IParseLogger
    {
        private readonly StringBuilder _log = new StringBuilder();
        private int _indentationLevel = 0;
        public void Log(string node, string data, int line, int column)
        {
            throw new NotImplementedException();
        }

        public void LogStart(string node, string data, int line, int column)
        {
            _log.Append(new string(' ', _indentationLevel * 2));
            _log.AppendLine($"+{node} ({data}) @{line}:{column}");
            _indentationLevel++;
        }

        public void LogEnd(string node, string data, int line, int column)
        {
            _indentationLevel--;
            _log.Append(new string(' ', _indentationLevel * 2));
            _log.AppendLine($"-{node} ({data}) @{line}:{column}");
        }

        public string GetLog() => _log.ToString();
    }
}
