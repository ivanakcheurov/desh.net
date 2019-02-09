using System;
using System.Collections.Generic;
using System.Text;

namespace Desh.Parsing.Ast
{
    public abstract class Node
    {
        public string DeshSpan { get; set; }

        public Node(string deshSpan)
        {
            DeshSpan = deshSpan;
        }
    }
}
