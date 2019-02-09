using System;

namespace Desh.Parsing.Ast
{
    public class Expression_OR_List : ExpressionBlock
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Expression_OR_List() : base(null)
        {
        }

        public Expression_OR_List(string deshSpan) : base(deshSpan)
        {
        }

        public Expression_AND_Mapping[] ExpressionAndMappings { get; set; }
    }
}