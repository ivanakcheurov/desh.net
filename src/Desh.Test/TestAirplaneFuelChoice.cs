using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Desh.Test
{
    public class TestAirplaneFuelChoice
    {
        private const string Grammar_020 = @"
- airline_ICAO:
    - DAL: AvGas
    - [CFL, TRA, TVF]: Mogas
    - .between: [KLA, KLZ]  
      then: Mogas
    - RYR:
        airport_IATA:
          - [LON, LHR, LGW]: AvGas
          - [LCY, STN, LTN, SEN]: Mogas
    ";
        [Theory]
        [MemberData(nameof(YamlTestReader.GetData), parameters: "AirplaneFuelChoice.tests.yaml", MemberType = typeof(YamlTestReader))]
        public void Grammar_v010a(string name, TestCaseSource source, string desh, Dictionary<string, string> vars, string expectedDecision)
        {
            ParseExecuteTestRunner.AssertPicksCorrectDecision(name, source, desh, vars, expectedDecision);
        }


        [Theory(Skip = "Not implemented")]
        [InlineData("{airline_ICAO: 'DAL'}", "AvGas")]
        [InlineData("{airline_ICAO: 'CFL'}", "Mogas")]
        [InlineData("{airline_ICAO: 'TRA'}", "Mogas")]
        [InlineData("{airline_ICAO: 'TVF'}", "Mogas")]
        [InlineData("{airline_ICAO: 'KLC'}", "Mogas")]
        [InlineData("{airline_ICAO: 'KLH'}", "Mogas")]
        [InlineData("{airline_ICAO: 'RYR', airport_IATA: 'LON'}", "AvGas")]
        [InlineData("{airline_ICAO: 'RYR', airport_IATA: 'LHR'}", "AvGas")]
        [InlineData("{airline_ICAO: 'RYR', airport_IATA: 'LGW'}", "AvGas")]
        [InlineData("{airline_ICAO: 'RYR', airport_IATA: 'LCY'}", "Mogas")]
        [InlineData("{airline_ICAO: 'RYR', airport_IATA: 'STN'}", "Mogas")]
        [InlineData("{airline_ICAO: 'RYR', airport_IATA: 'LTN'}", "Mogas")]
        [InlineData("{airline_ICAO: 'RYR', airport_IATA: 'SEN'}", "Mogas")]
        public void Grammar_v020(string contextJson, string expectedDecision)
        {
            ParseExecuteTestRunner.AssertPicksCorrectDecision(Grammar_020, contextJson, expectedDecision);
        }
    }
}
