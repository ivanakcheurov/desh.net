using System;

namespace Desh.Parsing.Ast
{
    public class ScalarValue : Comparator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public ScalarValue() : base(null, null)
        {
        }

        public ScalarValue(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        public string Value { get; set; }
    }
}