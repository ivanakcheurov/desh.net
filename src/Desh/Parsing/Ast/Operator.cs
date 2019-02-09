using System;

namespace Desh.Parsing.Ast
{
    public class Operator : Node
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Operator() : base(null, null)
        {
        }

        public Operator(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        public string Name { get; set; }
        public string[] Arguments { get; set; }
    }
}
