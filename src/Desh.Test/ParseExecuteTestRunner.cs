using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Desh.Execution;
using Desh.Parsing;
using Newtonsoft.Json;
using Xunit;

namespace Desh.Test
{
    class ParseExecuteTestRunner
    {
        public static void AssertPicksCorrectDecision(string desh, string contextJson, string expectedDecision)
        {
            var parser = new DeshParser();

            var ast = parser.Parse(desh);
            var context = JsonConvert.DeserializeObject<Dictionary<string, string>>(contextJson);

            var vars = new SmokeDeshParsingAndExecution.VariableEvaluator(context);
            var opDic = new Dictionary<string, Func<string, string[], bool>>
            {
                { ".equals", (varValue, allowedVariants) => allowedVariants.Any(variant => StringComparer.InvariantCultureIgnoreCase.Compare(varValue, variant) == 0) },
                { ".hasLength", (varValue, allowedLengths) => allowedLengths.Any(length => varValue.Length == int.Parse(length)) },
                { ".between", (varValue, maxMinRange) => 
                    StringComparer.InvariantCultureIgnoreCase.Compare(varValue, maxMinRange[0]) >= 0 
                    && StringComparer.InvariantCultureIgnoreCase.Compare(varValue, maxMinRange[1]) <= 0 },
            };
            var ops = new SmokeDeshParsingAndExecution.OperatorEvaluator(opDic);
            var engine = new Engine(vars, ops, true);
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
    }
}
