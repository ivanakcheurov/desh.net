using Desh.Execution;
using Desh.Parsing;
using System;
using System.Collections.Generic;

namespace Desh.OutOfTheBox
{
    public class LambdaOperatorEvaluator : IOperatorEvaluator, INameRecognizer
    {
        private readonly IReadOnlyDictionary<string, Func<string, string[], bool>> _operators;

        public LambdaOperatorEvaluator(IReadOnlyDictionary<string, Func<string, string[], bool>> operators)
        {
            _operators = operators;
        }

        public bool Evaluate(string variableValue, string operatorName, string[] arguments)
        {
            return _operators[operatorName](variableValue, arguments);
        }

        public bool Recognize(string name)
        {
            return _operators.ContainsKey(name);
        }
    }
}
