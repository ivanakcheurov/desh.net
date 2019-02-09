using System;

namespace Desh.Parsing.Ast
{
    public class ExpressionBlock : Node
    {
        [Obsolete("Should only be used by deserializers", true)]
        public ExpressionBlock() : base(null)
        {
        }

        public ExpressionBlock(string deshSpan) : base(deshSpan)
        {
        }
    }
}