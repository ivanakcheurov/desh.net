using System;
using System.Collections.Generic;
using System.Text;

namespace Desh.Parsing
{
    public interface IContext
    {
        IParseLogger Logger { get; }
        INameRecognizer VariableRecognizer { get; }
        INameRecognizer OperatorRecognizer { get; }
    }
}
