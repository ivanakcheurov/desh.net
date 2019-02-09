using System;

namespace Desh.Parsing.Ast
{
    public class ValueExpressionTree : Comparator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public ValueExpressionTree() : base(null, null)
        {
        }

        public ValueExpressionTree(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        public ScalarValue[] ScalarValues { get; set; }
        public ExpressionBlock ThenExpressionBlock { get; set; }
    }
}