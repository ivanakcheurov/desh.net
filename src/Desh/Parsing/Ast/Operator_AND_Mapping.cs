using System;
using System.Linq;

namespace Desh.Parsing.Ast
{
    // ReSharper disable once InconsistentNaming
    public class Operator_AND_Mapping : Comparator
    {
        public Operator_AND_Mapping(Operator[] operators, ExpressionBlock thenExpressionBlock, DecisionLeaf decision)
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

        public Operator[] Operators { get; }
        public ExpressionBlock ThenExpressionBlock { get; }
        public DecisionLeaf Decision { get; }
    }
}