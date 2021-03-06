﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Desh.Test
{
    public class TestServiceLevelAgreement
    {
        private const string Grammar_020 = @"
# a variable (like channel) can return several values like transport_type can return [ship, barge] or [train, freight train]
- domestic_request: 
    - yes:
        - channel: 
            - desh-trans.nl:
                  anyIsTrue:
                    - transport_type: ship
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-17 21:59:59]
                    - transport_type: 
                        - notEquals: ship
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-18 21:59:59]
          then: 2018-12-24
        
        - transport_type:
            - equalsAny:
                - ship
                - train
              then:
                channel: 
                  - [swedesh.se, desh-trans.fr]:
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-10 21:59:59]
                  - tradesh.no:
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-12 21:59:59]
                  - desh-trans.be:
                      service_paid_date:
                        - isBetween: [2018-12-10, 2018-12-14 21:59:59]
                  - [deshtra.co.uk, desh-trans.de]:
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-13 21:59:59]
                then: 2018-12-24
            - else:
                channel: 
                  - [swedesh.se, desh-trans.fr]:
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-11 21:59:59]
                  - tradesh.no:
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-13 21:59:59]
                  - [deshtra.co.uk, desh-trans.de, desh-trans.be]:
                      service_paid_date:
                        - isBetween: [2018-12-09, 2018-12-16 21:59:59]
                then: 2018-12-24
- transport_type: ship
  channel:
    - contains: inter
      then:
        destination_country:
          - [DE, UK, FR, IT, ES]:
              service_paid_date:
                - isBetween: [2018-12-09, 2018-12-09 21:59:59]
              then: 2018-12-21
          - [NL, BE]:
              service_paid_date:
                - isBetween: [2018-12-09, 2018-12-13 21:59:59]
              then: 2018-12-21
          - [NO, SE]:
              service_paid_date:
                - isBetween: [2018-12-01, 2018-12-06 21:59:59]
              then: 2018-12-21
              
- transport_type: ship
  channel:
    - contains: export
      then:
        destination_country:
          - [US, CA, AU, NZ]:
              service_paid_date:
                - isBetween: [2018-12-01, 2018-12-02 21:59:59]
              then: 2018-12-24
          - [NL, DE, UK]:
              service_paid_date:
                - isBetween: [2018-12-09, 2018-12-14 21:59:59]
              then: 2018-12-24
          - BE:
              service_paid_date:
                - isBetween: [2018-12-09, 2018-12-17 21:59:59]
              then: 2018-12-24
          - FR:
              service_paid_date:
                - isBetween: [2018-12-09, 2018-12-10 21:59:59]
              then: 2018-12-24
          - SE:
              service_paid_date:
                - isBetween: [2018-12-09, 2018-12-09 21:59:59]
              then: 2018-12-24
          - NO:
              service_paid_date:
                - isBetween: [2018-12-01, 2018-12-06 21:59:59]
              then: 2018-12-24
";
        [Theory]
        [MemberData(nameof(YamlTestReader.GetData), parameters: "SLA.tests.yaml", MemberType = typeof(YamlTestReader))]
        public async Task Grammar_v010a(string name, TestCaseSource source, string desh, Dictionary<string, string> vars, string expectedDecision)
        {
            await ParseExecuteTestRunner.AssertPicksCorrectDecision(name, source, desh, vars, expectedDecision);
        }

        [Theory(Skip = "Not implemented")]
        [InlineData(
            "{domestic_request: 'yes', service_paid_date: '2018-12-17 14:12:34', channel: 'desh-trans.nl', transport_type: 'ship'}",
            "2018-12-24")]
        public async Task Grammar_v020(string contextJson, string expectedDecision)
        {
            await ParseExecuteTestRunner.AssertPicksCorrectDecision(Grammar_020, contextJson, expectedDecision);
        }
    }
}
