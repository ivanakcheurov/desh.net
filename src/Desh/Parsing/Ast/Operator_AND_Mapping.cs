using System;
using System.Linq;

namespace Desh.Parsing.Ast
{
    // ReSharper disable once InconsistentNaming
    public class Operator_AND_Mapping : Comparator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Operator_AND_Mapping() : base(null)
        {
        }

        public Operator_AND_Mapping(Operator[] operators, ExpressionBlock thenExpressionBlock, DecisionLeaf decision, string deshSpan) : base(deshSpan)
        {
            if (operators == null || operators.Any() == false)
                throw new ArgumentException("Must contain at least 1 operator", nameof(operators));
            Operators = operators;
            if (thenExpressionBlock != null && decision != null)
            {
                // todo: consider allowing ThenExpressionBlock and Decision be specified at the same time
                throw new ArgumentException($"Only 1 among {nameof(thenExpressionBlock)} and {nameof(decision)} parameters can have non-null value");
            }
            ThenExpressionBlock = thenExpressionBlock;
            Decision = decision;
        }

        public Operator[] Operators { get; set; }
        public ExpressionBlock ThenExpressionBlock { get; set; }
        public DecisionLeaf Decision { get; set; }
    }
}