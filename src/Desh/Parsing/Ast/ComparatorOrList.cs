namespace Desh.Parsing.Ast
{
    public class ComparatorOrList : Comparator
    {
        public ComparatorOrList()
        {
        }

        public Comparator[] Comparators { get; set; }
    }
}