using System;

namespace Desh.Parsing.Ast
{
    public class ComparatorOrList : Comparator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public ComparatorOrList() : base(null)
        {
        }

        public ComparatorOrList(string deshSpan) : base(deshSpan)
        {
        }

        public Comparator[] Comparators { get; set; }
    }
}