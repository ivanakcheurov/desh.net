using System;
using System.Collections.Generic;
using System.Text;

namespace Desh.Parsing.Ast
{
    public abstract class Node
    {
        public string SourceDesh { get; set; }
        public string SourceDeshLocation { get; set; }

        public Node(string sourceDesh, string sourceDeshLocation)
        {
            SourceDesh = sourceDesh;
            SourceDeshLocation = sourceDeshLocation;
        }
    }
}
