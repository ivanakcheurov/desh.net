using System;

namespace Desh.Parsing.Ast
{
    public class Variable : Node
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Variable() : base(null, null)
        {
        }

        public Variable(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        public string Name { get; set; }
    }
}
