﻿using System;

namespace Desh.Parsing.Ast
{
    public class Operator : Node
    {
        [Obsolete("Should only be used by deserializers", true)]
        public Operator() : base(null)
        {
        }

        public Operator(string deshSpan) : base(deshSpan)
        {
        }

        public string Name { get; set; }
        public string[] Arguments { get; set; }
    }
}
