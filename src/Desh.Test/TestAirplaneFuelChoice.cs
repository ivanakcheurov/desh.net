using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Desh.Test
{
    public class TestAirplaneFuelChoice
    {
        [Theory]
        [MemberData(nameof(YamlTestReader.GetData), parameters: "AirplaneFuelChoice.tests.yaml", MemberType = typeof(YamlTestReader))]
        public void Grammar_v010a(string name, TestCaseSource source, string desh, Dictionary<string, string> vars, string expectedDecision)
        {
            ParseExecuteTestRunner.AssertPicksCorrectDecision(name, source, desh, vars, expectedDecision);
        }
    }
}
