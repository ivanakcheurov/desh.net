using System;

namespace Desh.Parsing.Ast
{
    public class ValueExpressionTree : Comparator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public ValueExpressionTree() : base(null)
        {
        }

        public ValueExpressionTree(string deshSpan) : base(deshSpan)
        {
        }

        public ScalarValue[] ScalarValues { get; set; }
        public ExpressionBlock ThenExpressionBlock { get; set; }
    }
}