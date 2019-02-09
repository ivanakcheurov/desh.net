using System;
using System.Collections.Generic;

namespace Desh.Parsing.Ast
{
    public class Expression_AND_Mapping : ExpressionBlock
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Expression_AND_Mapping() : base(null, null)
        {
        }

        public Expression_AND_Mapping(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        // todo: any is true
        // todo: consider allowing inspecting the same variable multiple times
        public Dictionary<Variable, Comparator> NormalPairs { get; set; }
        public ExpressionBlock ThenExpressionBlock { get; set; }
        public DecisionLeaf DecisionLeaf { get; set; }
    }
}