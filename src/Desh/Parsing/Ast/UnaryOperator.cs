using System;

namespace Desh.Parsing.Ast
{
    public class UnaryOperator : Comparator // inheritance is for an unary operator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public UnaryOperator() : base(null, null)
        {
        }

        public UnaryOperator(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        public string Name { get; set; }
    }
}