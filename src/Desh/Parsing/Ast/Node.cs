using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Desh.Parsing.Ast
{
    public abstract class Node
    {
        [YamlIgnore]
        public string SourceDesh { get; set; }
        public string SourceDeshLocation { get; set; }

        public Node(string sourceDesh, string sourceDeshLocation)
        {
            SourceDesh = sourceDesh;
            SourceDeshLocation = sourceDeshLocation;
        }
    }
}
