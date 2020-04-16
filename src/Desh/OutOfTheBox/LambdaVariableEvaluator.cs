using Desh.Execution;
using Desh.Parsing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Desh.OutOfTheBox
{
    public class LambdaVariableEvaluator : IVariableEvaluator, INameRecognizer
    {
        private readonly IReadOnlyDictionary<string, Func<Task<string>>> _variableValues;

        public LambdaVariableEvaluator(IReadOnlyDictionary<string, Func<Task<string>>> variableValues)
        {
            _variableValues = variableValues;
        }

        public Task<string> Evaluate(string variable)
        {
            return _variableValues[variable]();
        }

        public bool Recognize(string name)
        {
            return _variableValues.ContainsKey(name);
        }
    }
}
