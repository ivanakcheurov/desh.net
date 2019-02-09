using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Desh.Execution;
using Desh.OutOfTheBox;
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
            var facade = new DeshFacade();
            var actualDecision =
                facade.ParseAndMakeStringDecision(desh,
                    vars.ToImmutableDictionary(kvp => kvp.Key, kvp => new Func<string>(() => kvp.Value)),
                    null);

            
            if (expectedDecision == null)
            {
                Assert.Null(actualDecision);
            }
            else
            {
                Assert.Equal(expectedDecision, actualDecision);
            }
        }
    }
}
