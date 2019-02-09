using System;

namespace Desh.Parsing.Ast
{
    public class DecisionLeaf : ExpressionBlock
    {
        [Obsolete("Should only be used by deserializers", true)]
        public DecisionLeaf() : base(null, null)
        {
        }

        public DecisionLeaf(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        // todo: allow arbitrary objects. For now just a string
        public string Decision { get; set; }
    }
}