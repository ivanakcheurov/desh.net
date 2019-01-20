using System;
using System.Collections.Generic;
using System.Text;
using Desh.Parsing;

namespace Desh.Test
{
    public class Context : IContext
    {
        public Context(IParseLogger logger, INameRecognizer variableRecognizer, INameRecognizer operatorRecognizer)
        {
            Logger = logger;
            VariableRecognizer = variableRecognizer;
            OperatorRecognizer = operatorRecognizer;
        }

        public IParseLogger Logger { get; }
        public INameRecognizer VariableRecognizer { get; }
        public INameRecognizer OperatorRecognizer { get; }
    }
}
