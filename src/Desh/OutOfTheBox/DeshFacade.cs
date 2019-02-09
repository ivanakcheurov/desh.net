﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Desh.Execution;
using Desh.OutOfTheBox;
using Desh.Parsing;
using Desh.Parsing.Ast;
using YamlDotNet.Serialization;

// ReSharper disable once CheckNamespace
namespace Desh
{
    /// <summary>
    /// Makes parsing and executing desh simple
    /// </summary>
    public class DeshFacade
    {
        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in <paramref name="desh"/>
        /// </summary>
        /// <param name="desh">Business rules to make a decision</param>
        /// <param name="variableEvaluations">variables and how to get their values</param>
        /// <param name="customOperatorEvaluations">operators and how to apply them to a variable value and arguments</param>
        /// <returns>a single string decision obtained by executing business rules specified in desh</returns>
        public string ParseAndMakeStringDecision(string desh,
            IReadOnlyDictionary<string, Func<string>> variableEvaluations,
            IReadOnlyDictionary<string, Func<string, string[], bool>> customOperatorEvaluations)
        {
            return ParseAndMakeStringDecision(desh, variableEvaluations, customOperatorEvaluations, out string parseLog,
                out string abstractSyntaxTreeYaml, out string executionLogYaml);
        }

        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in <paramref name="desh"/>
        /// </summary>
        /// <param name="desh">Business rules to make a decision</param>
        /// <param name="variableEvaluations">variables and how to get their values</param>
        /// <param name="customOperatorEvaluations">operators and how to apply them to a variable value and arguments</param>
        /// <param name="parseLog">logs from the parser</param>
        /// <param name="abstractSyntaxTreeYaml">YAML version of the Abstract Syntax Tree parsed from <paramref name="desh"/></param>
        /// <param name="executionLogYaml">YAML version of the log after the engine has executed <paramref name="desh"/></param>
        /// <returns>a single string decision obtained by executing business rules specified in desh</returns>
        public string ParseAndMakeStringDecision(string desh,
            IReadOnlyDictionary<string, Func<string>> variableEvaluations,
            IReadOnlyDictionary<string, Func<string, string[], bool>> customOperatorEvaluations,
            out string parseLog, out string abstractSyntaxTreeYaml, out string executionLogYaml)
        {
            var parsedDeshAst = Parse(desh, variableEvaluations.Keys, customOperatorEvaluations?.Keys, out parseLog,
                out abstractSyntaxTreeYaml);

            var result = MakeStringDecision(parsedDeshAst, variableEvaluations, customOperatorEvaluations, out executionLogYaml);
            return result;
        }

        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in <paramref name="desh"/>
        /// </summary>
        /// <param name="desh">Business rules to make a decision</param>
        /// <param name="variableEvaluations">variables and how to get their values</param>
        /// <param name="customOperatorEvaluations">operators and how to apply them to a variable value and arguments</param>
        /// <returns>a True/False decision obtained by executing business rules specified in desh.
        /// Note: there desh should be designed in such a way to return only PositiveEval or nothing</returns>
        public bool ParseAndMakeBooleanDecision(string desh,
            IReadOnlyDictionary<string, Func<string>> variableEvaluations,
            IReadOnlyDictionary<string, Func<string, string[], bool>> customOperatorEvaluations)
        {
            return ParseAndMakeBooleanDecision(desh, variableEvaluations, customOperatorEvaluations, out string parseLog,
                out string abstractSyntaxTreeYaml, out string executionLogYaml);
        }

        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in <paramref name="desh"/>
        /// </summary>
        /// <param name="desh">Business rules to make a decision</param>
        /// <param name="variableEvaluations">variables and how to get their values</param>
        /// <param name="customOperatorEvaluations">operators and how to apply them to a variable value and arguments</param>
        /// <param name="parseLog">logs from the parser</param>
        /// <param name="abstractSyntaxTreeYaml">YAML version of the Abstract Syntax Tree parsed from <paramref name="desh"/></param>
        /// <param name="executionLogYaml">YAML version of the log after the engine has executed <paramref name="desh"/></param>
        /// <returns>a True/False decision obtained by executing business rules specified in desh.
        /// Note: there desh should be designed in such a way to return only PositiveEval or nothing</returns>
        public bool ParseAndMakeBooleanDecision(string desh,
            IReadOnlyDictionary<string, Func<string>> variableEvaluations,
            IReadOnlyDictionary<string, Func<string, string[], bool>> customOperatorEvaluations,
            out string parseLog, out string abstractSyntaxTreeYaml, out string executionLogYaml)
        {
            var parsedDeshAst = Parse(desh, variableEvaluations.Keys, customOperatorEvaluations?.Keys, out parseLog,
                out abstractSyntaxTreeYaml);

            var result = MakeBoolDecision(parsedDeshAst, variableEvaluations, customOperatorEvaluations, out executionLogYaml);
            return result;
        }

        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in the parsed Desh (<paramref name="parsedDeshAst"/>)
        /// </summary>
        /// <param name="parsedDeshAst">Parsed business rules to make a decision</param>
        /// <param name="variableEvaluations">variables and how to get their values</param>
        /// <param name="customOperatorEvaluations">operators and how to apply them to a variable value and arguments</param>
        /// <param name="executionLogYaml">YAML version of the log after the engine has executed <paramref name="parsedDeshAst"/></param>
        /// <returns>a single string decision obtained by executing business rules specified in desh</returns>
        public string MakeStringDecision(ExpressionBlock parsedDeshAst,
            IReadOnlyDictionary<string, Func<string>> variableEvaluations,
            IReadOnlyDictionary<string, Func<string, string[], bool>> customOperatorEvaluations,
            out string executionLogYaml)
        {
            var result = ExecuteInternal(parsedDeshAst, variableEvaluations, customOperatorEvaluations, out executionLogYaml);
            switch (result)
            {
                case Conclusion conclusion:
                    return conclusion.Decisions.Single();
                case null:
                    return null;
                default:
                    throw new InvalidOperationException("Unexpected type of EvaluationResult: " + result);
            }
        }

        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in the parsed Desh (<paramref name="parsedDeshAst"/>)
        /// </summary>
        /// <param name="parsedDeshAst">Parsed business rules to make a decision</param>
        /// <param name="variableEvaluations">variables and how to get their values</param>
        /// <param name="customOperatorEvaluations">operators and how to apply them to a variable value and arguments</param>
        /// <param name="executionLogYaml">YAML version of the log after the engine has executed <paramref name="parsedDeshAst"/></param>
        /// <returns>a True/False decision obtained by executing business rules specified in desh.
        /// Note: there desh should be designed in such a way to return only PositiveEval or nothing</returns>
        public bool MakeBoolDecision(ExpressionBlock parsedDeshAst,
            IReadOnlyDictionary<string, Func<string>> variableEvaluations,
            IReadOnlyDictionary<string, Func<string, string[], bool>> customOperatorEvaluations,
            out string executionLogYaml)
        {
            var result = ExecuteInternal(parsedDeshAst, variableEvaluations, customOperatorEvaluations, out executionLogYaml);
            switch (result)
            {
                case PositiveEval _:
                    return true;
                case null:
                    return false;
                default:
                    throw new InvalidOperationException("Unexpected type of EvaluationResult: " + result);
            }
        }

        private EvaluationResult ExecuteInternal(ExpressionBlock parsedDeshAst,
            IReadOnlyDictionary<string, Func<string>> variableEvaluations,
            IReadOnlyDictionary<string, Func<string, string[], bool>> customOperatorEvaluations,
            out string executionLogYaml)
        {
            var variableEvaluator = new LambdaVariableEvaluator(variableEvaluations);
            var operators = StandardOperators.Get().Concat(customOperatorEvaluations ?? Enumerable.Empty<KeyValuePair<string, Func<string, string[], bool>>>())
                .ToDictionary(_ => _.Key, _ => _.Value);
            var operatorEvaluator = new LambdaOperatorEvaluator(operators);

            var executionLogger = new ExecutionLogger();
            var engine = new Engine(variableEvaluator, operatorEvaluator, executionLogger, true);
            executionLogger.Initialize(parsedDeshAst.SourceDesh, engine);
            var result = engine.Execute(parsedDeshAst);
            executionLogYaml = new SerializerBuilder().Build().Serialize(executionLogger.GetLog());
            return result;
        }

        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in <paramref name="desh"/>
        /// </summary>
        /// <param name="desh">Business rules to make a decision</param>
        /// <param name="knownVariables">variables that may be referenced in desh</param>
        /// <param name="customOperators">operators that may be used in desh</param>
        /// <returns>a single string decision obtained by executing business rules specified in desh</returns>
        public ExpressionBlock Parse(string desh,
            IEnumerable<string> knownVariables,
            IEnumerable<string> customOperators)
        {
            var ast = Parse(desh, knownVariables, customOperators, out string parseLog,
                out string abstractSyntaxTreeYaml);
            return ast;
        }

        /// <summary>
        /// Given variables and operators, it returns a decision (string) according to rules specified in <paramref name="desh"/>
        /// </summary>
        /// <param name="desh">Business rules to make a decision</param>
        /// <param name="knownVariables">variables that may be referenced in desh</param>
        /// <param name="customOperators">operators that may be used in desh</param>
        /// <param name="parseLog">logs from the parser</param>
        /// <param name="abstractSyntaxTreeYaml">YAML version of the Abstract Syntax Tree parsed from <paramref name="desh"/></param>
        /// <returns>a single string decision obtained by executing business rules specified in desh</returns>
        public ExpressionBlock Parse(string desh,
            IEnumerable<string> knownVariables,
            IEnumerable<string> customOperators,
            out string parseLog, out string abstractSyntaxTreeYaml)
        {
            var parser = new DeshParser();
            var variableEvaluator = new NameRecognizer(knownVariables);
            var operatorEvaluator = new NameRecognizer(StandardOperators.Get().Keys.Concat(customOperators ?? Enumerable.Empty<string>()));
            var parseLogger = new ParseLogger();
            var ctx = new Context(desh, parseLogger, variableEvaluator, operatorEvaluator);
            var ast = parser.Parse(desh, ctx);
            parseLog = parseLogger.GetLog();
            var serializerBuilder = new SerializerBuilder();
            var nodeTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(Node).IsAssignableFrom(assemblyType)
                select assemblyType).ToArray();
            foreach (var nodeType in nodeTypes)
            {
                serializerBuilder.WithTagMapping($"!{nodeType.Name}", nodeType);
            }
            var serializer = serializerBuilder.EnsureRoundtrip().Build();
            abstractSyntaxTreeYaml = serializer.Serialize(ast);

            return ast;
        }

        public class NameRecognizer : INameRecognizer
        {
            private HashSet<string> _knownNames;

            public NameRecognizer(IEnumerable<string> knownNames)
            {
                _knownNames = new HashSet<string>(knownNames);
            }

            public bool Recognize(string name)
            {
                return _knownNames.Contains(name);
            }
        }
    }
}