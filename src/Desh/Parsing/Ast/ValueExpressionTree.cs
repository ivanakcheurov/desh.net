namespace Desh.Parsing.Ast
{
    public class ValueExpressionTree : Comparator
    {
        public ValueExpressionTree()
        {
        }

        public ScalarValue[] ScalarValues { get; set; }
        public ExpressionBlock ThenExpressionBlock { get; set; }
    }
}