﻿namespace Desh.Parsing.Ast
{
    public class Expression_OR_List : ExpressionBlock
    {
        public Expression_OR_List()
        {
        }

        public Expression_AND_Mapping[] ExpressionAndMappings { get; set; }
    }
}