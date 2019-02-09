using System;

namespace Desh.Parsing.Ast
{
    public class Expression_OR_List : ExpressionBlock
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Expression_OR_List() : base(null, null)
        {
        }

        public Expression_OR_List(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        public Expression_AND_Mapping[] ExpressionAndMappings { get; set; }
    }
}