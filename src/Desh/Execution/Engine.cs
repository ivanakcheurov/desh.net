using Desh.Parsing.Ast;

namespace Desh.Execution
{
    public class Engine
    {
        private readonly IVariableEvaluator _variableEvaluator;
        private readonly IOperatorEvaluator _operatorEvaluator;
        private bool _stopOnFirstAcceptedDecision;

        public Engine(IVariableEvaluator variableEvaluator, IOperatorEvaluator operatorEvaluator, bool stopOnFirstAcceptedDecision)
        {
            _variableEvaluator = variableEvaluator;
            _operatorEvaluator = operatorEvaluator;
            _stopOnFirstAcceptedDecision = stopOnFirstAcceptedDecision;
        }

        public EvaluationResult Execute(ExpressionBlock expressionBlock)
        {
            switch (expressionBlock)
            {
                case DecisionLeaf decision:
                    return new Conclusion(new[] {decision.Decision});
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
                var variableValue = _variableEvaluator.Evaluate(variable);
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
                return new Conclusion(new[]{expressionAndMapping.DecisionLeaf.Decision});
            return new PositiveEval();
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
                    var res = _operatorEvaluator.Evaluate(variableValue, ".equals", new[] { scalar.Value });
                    if (res)
                        return new PositiveEval();
                    return null;
                case UnaryOperator op: // only unary operator here
                    var res2 = _operatorEvaluator.Evaluate(variableValue, op.Name, null);
                    if (res2)
                        return new PositiveEval();
                    return null;

                default:
                    throw new ExecutionException("Unexpected to encounter a comparator of type: " + comparator.GetType().FullName);
            }
        }

        public EvaluationResult Execute(string variableValue, ValueExpressionTree valueExpressionTree)
        {
            foreach (var scalarValue in valueExpressionTree.ScalarValues)
            {
                var match = _operatorEvaluator.Evaluate(variableValue, ".equals", new []{ scalarValue.Value });
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
                
                var match = _operatorEvaluator.Evaluate(variableValue, @operator.Name, @operator.Arguments);
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
                return new Conclusion(new[] { operatorAndMapping.Decision.Decision });
            }
            return new PositiveEval();
        }
    }
}