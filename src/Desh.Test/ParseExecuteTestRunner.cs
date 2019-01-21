using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Desh.Execution;
using Desh.Parsing;
using Desh.Parsing.Ast;
using Newtonsoft.Json;
using Xunit;
using YamlDotNet.Serialization;

namespace Desh.Test
{
    class ParseExecuteTestRunner
    {
        public static void AssertPicksCorrectDecision(string desh, string contextJson, string expectedDecision)
        {
            var vars = JsonConvert.DeserializeObject<Dictionary<string, string>>(contextJson);
            AssertPicksCorrectDecision(null, null, desh, vars, expectedDecision);
        }
        public static void AssertPicksCorrectDecision(string name, TestCaseSource source, string desh, Dictionary<string, string> vars, string expectedDecision)
        {
            var parser = new DeshParser();


            var varsEv = new SmokeDeshParsingAndExecution.VariableEvaluator(vars);
            var opDic = new Dictionary<string, Func<string, string[], bool>>
            {
                { ".equals", Equal },
                { "notEquals", (varValue, allowedLengths) => !Equal(varValue, allowedLengths) },
                { ".hasLength", (varValue, allowedLengths) => allowedLengths.Any(length => varValue.Length == int.Parse(length)) },
                { ".between", IsBetween },
                { "isBetween", IsBetween },
                { "equalsAny", Equal },
                { "contains", Contains },
            };
            var ops = new SmokeDeshParsingAndExecution.OperatorEvaluator(opDic);
            var parseLogger = new ParseLogger();
            var ctx = new Context(parseLogger, varsEv, ops);
            var ast = parser.Parse(desh, ctx);
            var astJson = JsonConvert.SerializeObject(ast);
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
            var astYaml = serializer.Serialize(ast);

            var engine = new Engine(varsEv, ops, true);
            var result = engine.Execute(ast);
            if (expectedDecision == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.Equal(expectedDecision, Assert.Single(Assert.IsType<Conclusion>(result).Decisions));
            }
        }

        private static bool Equal(string varValue, string[] allowedVariants)
        {
            return allowedVariants.Any(variant => StringComparer.InvariantCultureIgnoreCase.Compare(varValue, variant) == 0);
        }
        private static bool Contains(string varValue, string[] searchedSubstrings)
        {
            return searchedSubstrings.Any(variant => varValue.Contains(variant, StringComparison.InvariantCultureIgnoreCase));
        }

        private static bool IsBetween(string varValue, string[] maxMinRange)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(varValue, maxMinRange[0]) >= 0 && StringComparer.InvariantCultureIgnoreCase.Compare(varValue, maxMinRange[1]) <= 0;
        }
    }
}
