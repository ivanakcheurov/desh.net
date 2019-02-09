using System;

namespace Desh.Parsing.Ast
{
    public class DecisionLeaf : ExpressionBlock
    {
        [Obsolete("Should only be used by deserializers", true)]
        public DecisionLeaf() : base(null)
        {
        }

        public DecisionLeaf(string deshSpan) : base(deshSpan)
        {
        }

        // todo: allow arbitrary objects. For now just a string
        public string Decision { get; set; }
    }
}