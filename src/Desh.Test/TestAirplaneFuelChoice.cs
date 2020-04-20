using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Desh.Test
{
    public class TestAirplaneFuelChoice
    {
        [Theory]
        [MemberData(nameof(YamlTestReader.GetData), parameters: "AirplaneFuelChoice.tests.yaml", MemberType = typeof(YamlTestReader))]
        public async Task Grammar_v010a(string name, TestCaseSource source, string desh, Dictionary<string, string> vars, string expectedDecision)
        {
            await ParseExecuteTestRunner.AssertPicksCorrectDecision(name, source, desh, vars, expectedDecision);
        }
    }
}
