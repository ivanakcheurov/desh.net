using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Desh.Execution;
using Desh.Parsing;

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
