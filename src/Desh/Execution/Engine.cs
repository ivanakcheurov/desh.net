using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private int _currentExecutionLogStepNumber = 1;

        public Engine(IVariableEvaluator variableEvaluator, IOperatorEvaluator operatorEvaluator, IExecutionLogger executionLogger)
        {
            _variableEvaluator = variableEvaluator;
            _operatorEvaluator = operatorEvaluator;
            _executionLogger = executionLogger;
        }

        public async Task<EvaluationResult> Execute(ExpressionBlock expressionBlock)
        {
            switch (expressionBlock)
            {
                case DecisionLeaf decision:
                    return LogReturn(MakeConclusion(decision));
                case Expression_OR_List list:
                    foreach (var expressionAndMapping in list.ExpressionAndMappings)
                    {
                        var result = await Execute(expressionAndMapping);
                        switch (result)
                        {
                            case null: break; // just continue to the next node
                            case Conclusion conclusion: return LogReturn(conclusion); // todo: implement stopOnFirst = false
                            // todo: consider what to do if expressionAndMapping (that contain decisions) are mixed with pure bool expressionAndMapping (e.g. that have only comparisons with ScalarValue)
                            case PositiveEval positive: return LogReturn(positive); 
                            default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                        }
                    }
                    return LogReturn(null);
                case Expression_AND_Mapping map:
                    return LogReturn(await Execute(map));
                default: throw new ExecutionException("Unexpected result of type: " + expressionBlock.GetType().FullName);
            }
        }

        public async Task<EvaluationResult> Execute(Expression_AND_Mapping expressionAndMapping)
        {
            foreach (var expressionPair in expressionAndMapping.NormalPairs)
            {
                // todo: anyIsTrue
                var variable = expressionPair.Key;
                var variableValue = await EvaluateVariable(variable);
                var comparator = expressionPair.Value;
                var result = await Execute(variableValue, comparator);
                switch (result)
                {
                    case null: return LogReturn(null);
                    case Conclusion conclusion: return LogReturn(conclusion); // todo: implement stopOnFirst = false
                    case PositiveEval _: break; // just continue to the next node
                    default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                }
            }

            if (expressionAndMapping.ThenExpressionBlock != null)
            {
                var result = await Execute(expressionAndMapping.ThenExpressionBlock);
                switch (result) //todo: remove duplication
                {
                    case null: return LogReturn(null);
                    case Conclusion conclusion: return LogReturn(conclusion); // todo: implement stopOnFirst = false
                    case PositiveEval _: break; // just continue to the next node
                    default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                }
            }
            if (expressionAndMapping.DecisionLeaf != null)
                return LogReturn(MakeConclusion(expressionAndMapping.DecisionLeaf));
            return LogReturn(MarkAsPositive(expressionAndMapping));
        }

        public async Task<EvaluationResult> Execute(string variableValue, Comparator comparator)
        {
            switch (comparator)
            {
                case ComparatorOrList list:
                    foreach (var comp in list.Comparators)
                    {
                        var result = await Execute(variableValue, comp);
                        switch(result)
                        {
                            case null: break; // just continue to the next node
                            case Conclusion conclusion: return LogReturn(conclusion); // todo: implement stopOnFirst = false
                            // todo: consider what to do if comparators (that contain decisions) are mixed with pure bool comparators (like ScalarValue)
                            case PositiveEval positive: return LogReturn(positive); 
                            default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                        }
                    }
                    return LogReturn(null);
                case ValueExpressionTree valExp:
                    return LogReturn(await Execute(variableValue, valExp));
                case Operator_AND_Mapping mapping:
                    return LogReturn(await Execute(variableValue, mapping));
                case ScalarValue scalar:
                    var res = EvaluateOperator(variableValue, ".equals", new[] { scalar.Value }, scalar.SourceDeshLocation);
                    if (res)
                        return LogReturn(MarkAsPositive(scalar));
                    return LogReturn(null);
                case UnaryOperator op: // only unary operator here
                    var res2 = EvaluateOperator(variableValue, op.Name, null, op.SourceDeshLocation);
                    if (res2)
                        return LogReturn(MarkAsPositive(op));
                    return LogReturn(null);

                default:
                    throw new ExecutionException("Unexpected to encounter a comparator of type: " + comparator.GetType().FullName);
            }
        }

        public async Task<EvaluationResult> Execute(string variableValue, ValueExpressionTree valueExpressionTree)
        {
            foreach (var scalarValue in valueExpressionTree.ScalarValues)
            {
                var match = EvaluateOperator(variableValue, ".equals", new []{ scalarValue.Value }, scalarValue.SourceDeshLocation);
                if (match)
                {
                    return LogReturn(await Execute(valueExpressionTree.ThenExpressionBlock));
                }
            }
            return LogReturn(null);
        }

        public async Task<EvaluationResult> Execute(string variableValue, Operator_AND_Mapping operatorAndMapping)
        {
            foreach (var @operator in operatorAndMapping.Operators)
            {
                var match = EvaluateOperator(variableValue, @operator.Name, @operator.Arguments, @operator.SourceDeshLocation);
                if (match == false)
                {
                    return LogReturn(null);
                }
            }

            // todo: consider allowing ThenExpressionBlock and Decision be specified at the same time
            //
            if (operatorAndMapping.ThenExpressionBlock != null)
            {
                var result = await Execute(operatorAndMapping.ThenExpressionBlock);
                switch (result)
                {
                    case null: return LogReturn(null);
                    case Conclusion conclusion: return LogReturn(conclusion); // todo: implement stopOnFirst = false
                    case PositiveEval _: break; // just continue to the next node (possible 'decide')
                    default: throw new ExecutionException("Unexpected result of type: " + result.GetType().FullName);
                }
            }

            if (operatorAndMapping.Decision != null)
            {
                return LogReturn(MakeConclusion(operatorAndMapping.Decision));
            }
            return LogReturn(MarkAsPositive(operatorAndMapping));
        }

        private EvaluationResult LogReturn(EvaluationResult evaluationResult, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = null)
        {
            var step = new Step
            {
                Number = _currentExecutionLogStepNumber++,
                Timestamp = DateTime.UtcNow,
                SourceLocation = $"{callerFilePath}:{callerLineNumber}",
                Type = StepType.ReturnExpressionResult,
            };

            step.Result = evaluationResult switch
            {
                Conclusion conclusion => conclusion.Decisions.Single(),
                PositiveEval _ => nameof(PositiveEval),
                null => "<null>",
                _ => throw new InvalidOperationException("Unexpected evaluation result: " + evaluationResult)
            };
            _executionLogger.AddStep(step);
            return evaluationResult;
        }

        private async Task<string> EvaluateVariable(Variable variable,
            [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMemberName = null)
        {
            var step = new Step
            {
                Number = _currentExecutionLogStepNumber++,
                Timestamp = DateTime.UtcNow,
                DeshSpan = variable.SourceDeshLocation,
                SourceLocation = $"{callerFilePath}:{callerLineNumber}",
                Type = StepType.ExpandVariable,
                VariableName = variable.Name,
            };
            try
            {
                var variableValue = await _variableEvaluator.Evaluate(variable.Name);
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
                DeshSpan = decision.SourceDeshLocation,
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
                DeshSpan = node.SourceDeshLocation,
                SourceLocation = $"{callerFilePath}:{callerLineNumber}",
                Type = StepType.MarkAsPositive,
            };
            _executionLogger.AddStep(step);
            return new PositiveEval();
        }
    }
}