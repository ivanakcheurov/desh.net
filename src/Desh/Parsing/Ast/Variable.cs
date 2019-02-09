using System;
using System.Collections.Generic;
using System.Text;

namespace Desh.Parsing.Ast
{
    public class Variable : Node
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Variable() : base(null)
        {
        }

        public Variable(string deshSpan) : base(deshSpan)
        {
        }

        public string Name { get; set; }
    }
}
