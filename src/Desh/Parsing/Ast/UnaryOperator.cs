using System;

namespace Desh.Parsing.Ast
{
    public class UnaryOperator : Comparator // inheritance is for an unary operator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public UnaryOperator() : base(null)
        {
        }

        public UnaryOperator(string deshSpan) : base(deshSpan)
        {
        }

        public string Name { get; set; }
    }
}