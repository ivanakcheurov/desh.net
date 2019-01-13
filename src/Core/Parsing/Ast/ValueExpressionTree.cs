namespace Desh.Core.Parsing.Ast
{
    public class ValueExpressionTree : Comparator
    {
        public ScalarValue[] ScalarValues { get; set; }
        public ExpressionBlock ThenExpressionBlock { get; set; }
    }
}