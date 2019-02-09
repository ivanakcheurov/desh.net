using System;
using System.Runtime.CompilerServices;
using Desh.Execution.Logging;
using Desh.Parsing.Ast;
#pragma warning disable 1591

namespace Desh.Execution
{
    public class Engine
    {
        private readonly IVariableEvaluator _variableEvaluator;
        private readonly IOperatorEvaluator _operatorEvaluator;
        private readonly IExecutionLogger _executionLogger;
        private bool _stopOnFirstAcceptedDecision;
        private int _currentExecutionLogStepNumber = 1;

        public Engine(IVariableEvaluator variableEvaluator, IOperatorEvaluator operatorEvaluator, IExecutionLogger executionLogger, bool stopOnFirstAcceptedDecision)
        {
            _variableEvaluator = variableEvaluator;
            _operatorEvaluator = operatorEvaluator;
            _executionLogger = executionLogger;
            _stopOnFirstAcceptedDecision = stopOnFirstAcceptedDecision;
        }

        public EvaluationResult Execute(ExpressionBlock expressionBlock)
        {
            switch (expressionBlock)
            {
                case DecisionLeaf decision:
                    return MakeConclusion(decision);
                case Expression_OR_List list:
                    foreach (var expressionAndMapping in list.ExpressionAndMappings)
                    {
                        var result = Execute(expressionAndMapping);
                        switch (result)
                        {
                            case null: break; // just continue to the next node
                            case Conclusion conclusion: return conclusion; // todo: implement stopOnFirst = false
                            // todo: consider what to do if expressionAndMapping (that contain decisions) are mixed with pure bool expressionAndMapping (e.g. that have only comparisons with ScalarValue)
                            case PositiveEval positive: return positive; 
                            default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                        }
                    }
                    return null;
                case Expression_AND_Mapping map:
                    return Execute(map);
                default: throw new ExecutionException("Unexpected result of type: " + expressionBlock.GetType().FullName);
            }
        }

        public EvaluationResult Execute(Expression_AND_Mapping expressionAndMapping)
        {
            foreach (var expressionPair in expressionAndMapping.NormalPairs)
            {
                // todo: anyIsTrue
                var variable = expressionPair.Key;
                var variableValue = EvaluateVariable(variable);
                var comparator = expressionPair.Value;
                var result = Execute(variableValue, comparator);
                switch (result)
                {
                    case null: return null;
                    case Conclusion conclusion: return conclusion; // todo: implement stopOnFirst = false
                    case PositiveEval _: break; // just continue to the next node
                    default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                }
            }

            if (expressionAndMapping.ThenExpressionBlock != null)
            {
                var result = Execute(expressionAndMapping.ThenExpressionBlock);
                switch (result) //todo: remove duplication
                {
                    case null: return null;
                    case Conclusion conclusion: return conclusion; // todo: implement stopOnFirst = false
                    case PositiveEval _: break; // just continue to the next node
                    default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                }
            }
            if (expressionAndMapping.DecisionLeaf != null)
                return MakeConclusion(expressionAndMapping.DecisionLeaf);
            return MarkAsPositive(expressionAndMapping);
        }

        public EvaluationResult Execute(string variableValue, Comparator comparator)
        {
            switch (comparator)
            {
                case ComparatorOrList list:
                    foreach (var comp in list.Comparators)
                    {
                        var result = Execute(variableValue, comp);
                        switch(result)
                        {
                            case null: break; // just continue to the next node
                            case Conclusion conclusion: return conclusion; // todo: implement stopOnFirst = false
                            // todo: consider what to do if comparators (that contain decisions) are mixed with pure bool comparators (like ScalarValue)
                            case PositiveEval positive: return positive; 
                            default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                        }
                    }
                    return null;
                case ValueExpressionTree valExp:
                    return Execute(variableValue, valExp);
                case Operator_AND_Mapping mapping:
                    return Execute(variableValue, mapping);
                case ScalarValue scalar:
                    var res = EvaluateOperator(variableValue, ".equals", new[] { scalar.Value }, scalar.DeshSpan);
                    if (res)
                        return MarkAsPositive(scalar);
                    return null;
                case UnaryOperator op: // only unary operator here
                    var res2 = EvaluateOperator(variableValue, op.Name, null, op.DeshSpan);
                    if (res2)
                        return MarkAsPositive(op);
                    return null;

                default:
                    throw new ExecutionException("Unexpected to encounter a comparator of type: " + comparator.GetType().FullName);
            }
        }

        public EvaluationResult Execute(string variableValue, ValueExpressionTree valueExpressionTree)
        {
            foreach (var scalarValue in valueExpressionTree.ScalarValues)
            {
                var match = EvaluateOperator(variableValue, ".equals", new []{ scalarValue.Value }, scalarValue.DeshSpan);
                if (match)
                {
                    return Execute(valueExpressionTree.ThenExpressionBlock);
                }
            }
            return null;
        }

        public EvaluationResult Execute(string variableValue, Operator_AND_Mapping operatorAndMapping)
        {
            foreach (var @operator in operatorAndMapping.Operators)
            {
                
                var match = EvaluateOperator(variableValue, @operator.Name, @operator.Arguments, @operator.DeshSpan);
                if (match == false)
                {
                    return null;
                }
            }
            
            // todo: consider allowing ThenExpressionBlock and Decision be specified at the same time
            //
            if (operatorAndMapping.ThenExpressionBlock != null)
            {
                var result = Execute(operatorAndMapping.ThenExpressionBlock);
                switch (result)
                {
                    case null: return null;
                    case Conclusion conclusion: return conclusion; // todo: implement stopOnFirst = false
                    case PositiveEval _: break; // just continue to the next node (possible 'decide')
                    default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                }
            }

            if (operatorAndMapping.Decision != null)
            {
                return MakeConclusion(operatorAndMapping.Decision);
            }
            return MarkAsPositive(operatorAndMapping);
        }

        private string EvaluateVariable(Variable variable,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = null)
        {
            var step = new Step
            {
                Number = _currentExecutionLogStepNumber++,
                Timestamp = DateTime.UtcNow,
                DeshSpan = variable.DeshSpan,
                SourceLocation = $"{callerFilePath}:{callerLineNumber}",
                Type = StepType.ExpandVariable,
                VariableName = variable.Name,
            };
            try
            {
                var variableValue = _variableEvaluator.Evaluate(variable.Name);
                step.VariableValue = variableValue;
                return variableValue;
            }
            catch (Exception e)
            {
                step.Exception = e.ToString();
                throw;
            }
            finally
            {
                _executionLogger.AddStep(step);
            }
        }

        private bool EvaluateOperator(string variableValue, string operatorName, string[] arguments,
            string deshSpan,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = null)
        {
            var step = new Step
            {
                Number = _currentExecutionLogStepNumber++,
                Timestamp = DateTime.UtcNow,
                DeshSpan = deshSpan,
                SourceLocation = $"{callerFilePath}:{callerLineNumber}",
                Type = StepType.EvaluateOperator,
                OperatorName = operatorName,
                OperatorVariableValue = variableValue,
                OperatorArguments = arguments,
            };
            try
            {
                var result = _operatorEvaluator.Evaluate(variableValue, operatorName, arguments);
                step.OperatorResult = result;
                return result;
            }
            catch (Exception e)
            {
                step.Exception = e.ToString();
                throw;
            }
            finally
            {
                _executionLogger.AddStep(step);
            }
        }

        private Conclusion MakeConclusion(DecisionLeaf decision,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = null)
        {
            var step = new Step
            {
                Number = _currentExecutionLogStepNumber++,
                Timestamp = DateTime.UtcNow,
                DeshSpan = decision.DeshSpan,
                SourceLocation = $"{callerFilePath}:{callerLineNumber}",
                Type = StepType.MakeConclusion,
                Decision = decision.Decision,
            };
            _executionLogger.AddStep(step);
            var result = new Conclusion(new []{decision.Decision});
            return result;
        }

        private PositiveEval MarkAsPositive(Node node,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = null)
        {
            var step = new Step
            {
                Number = _currentExecutionLogStepNumber++,
                Timestamp = DateTime.UtcNow,
                DeshSpan = node.DeshSpan,
                SourceLocation = $"{callerFilePath}:{callerLineNumber}",
                Type = StepType.MarkAsPositive,
            };
            _executionLogger.AddStep(step);
            return new PositiveEval();
        }
    }
}