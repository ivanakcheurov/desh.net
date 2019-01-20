namespace Desh.Parsing.Ast
{
    public class DecisionLeaf : Node
    {
        public DecisionLeaf()
        {
        }

        // todo: allow arbitrary objects. For now just a string
        public string Decision { get; set; }
    }
}