using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xunit;

namespace Desh.Test
{
    class ParseExecuteTestRunner
    {
        public static async Task AssertPicksCorrectDecision(string desh, string contextJson, string expectedDecision)
        {
            var vars = JsonConvert.DeserializeObject<Dictionary<string, string>>(contextJson);
            await AssertPicksCorrectDecision(null, null, desh, vars, expectedDecision);
        }
        public static async Task AssertPicksCorrectDecision(string name, TestCaseSource source, string desh, Dictionary<string, string> vars, string expectedDecision)
        {
            var facade = new DeshFacade();
            var actualDecision =
                await facade.ParseAndMakeStringDecision(desh,
                    vars.ToImmutableDictionary(kvp => kvp.Key, kvp => new Func<Task<string>>(() => Task.FromResult(kvp.Value))),
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
