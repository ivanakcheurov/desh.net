using System;
using System.Collections.Generic;
using System.Text;
using Desh.Execution;
using Desh.Parsing;

namespace Desh.OutOfTheBox
{
    public class LambdaVariableEvaluator : IVariableEvaluator, INameRecognizer
    {
        private readonly IReadOnlyDictionary<string, Func<string>> _variableValues;

        public LambdaVariableEvaluator(IReadOnlyDictionary<string, Func<string>> variableValues)
        {
            _variableValues = variableValues;
        }

        public string Evaluate(string variable)
        {
            return _variableValues[variable]();
        }

        public bool Recognize(string name)
        {
            return _variableValues.ContainsKey(name);
        }
    }
}
