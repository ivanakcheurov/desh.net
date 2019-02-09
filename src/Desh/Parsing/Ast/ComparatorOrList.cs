using System;

namespace Desh.Parsing.Ast
{
    public class ComparatorOrList : Comparator
    {
        [Obsolete("Should only be used by deserializers", true)]
        public ComparatorOrList() : base(null, null)
        {
        }

        public ComparatorOrList(string sourceDesh, string sourceDeshLocation) : base(sourceDesh, sourceDeshLocation)
        {
        }

        public Comparator[] Comparators { get; set; }
    }
}