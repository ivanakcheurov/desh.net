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
        private const string Grammar_010 = @"
- airline_ICAO:
    - DAL:
        decide: AvGas
    - .between: [KLA, KLZ]  
      decide: Mogas
    - [CFL, TRA, TVF]: 
        decide: Mogas
    - RYR:
        airport_IATA:
          - [LON, LHR, LGW]:
              decide: AvGas
          - [LCY, STN, LTN, SEN]:
              decide: Mogas
    ";
        [Theory]
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
        public void Grammar_v010(string contextJson, string expectedDecision)
        {
            ParseExecuteTestRunner.AssertPicksCorrectDecision(Grammar_010, contextJson, expectedDecision);
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
