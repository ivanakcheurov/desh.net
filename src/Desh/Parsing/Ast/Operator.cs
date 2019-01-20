namespace Desh.Parsing.Ast
{
    public class Operator : Node
    {
        public Operator()
        {
        }

        public string Name { get; set; }
        public string[] Arguments { get; set; }
    }
}
