using System;

namespace Desh.Parsing.Ast
{
    public class ScalarValue : Comparator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public ScalarValue() : base(null)
        {
        }

        public ScalarValue(string deshSpan) : base(deshSpan)
        {
        }

        public string Value { get; set; }
    }
}