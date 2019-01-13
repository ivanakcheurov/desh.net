using System.Collections.Generic;

namespace Desh.Core.Parsing.Ast
{
    public class Expression_AND_Mapping : ExpressionBlock
    {
        // todo: any is true
        // todo: consider allowing inspecting the same variable multiple times
        public Dictionary<string, Comparator> NormalPairs { get; set; }
        public DecisionLeaf DecisionLeaf { get; set; }
    }
}