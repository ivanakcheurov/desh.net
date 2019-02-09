using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core.Events;

namespace Desh.Parsing
{
    public static class Extensions
    {
        public static string ToDeshSpan(this ParsingEvent parsingEvent)
        {
            return ToDeshSpan(parsingEvent, parsingEvent);
        }

        public static string ToDeshSpan(ParsingEvent startEvent, ParsingEvent endEvent)
        {
            return $"{startEvent.Start.Line}:{startEvent.Start.Column} - {endEvent.End.Line}:{endEvent.End.Column}";
        }
    }
}
