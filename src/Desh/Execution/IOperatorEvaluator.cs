﻿namespace Desh.Execution
{
    public interface IOperatorEvaluator
    {
        bool Evaluate(string variableValue, string operatorName, string[] arguments);
    }
}