using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Desh.Test
{
    public class TestServiceLevelAgreement
    {
        private const string Grammar_010 = @"
# a variable (like channel) can return several values like transport_type can return [ship, barge] or [train, freight train]
- domestic_request: 
    - yes:
        - channel: 
            - desh-trans.nl:
                - transport_type: ship
                  service_paid_date:
                    - isBetween: [2018-12-09, '2018-12-17 21:59:59']
                - transport_type: 
                    - notEquals: ship
                  service_paid_date:
                    - isBetween: [2018-12-09, '2018-12-18 21:59:59']
          decide: '2018-12-24'
        
        - transport_type:
            - equalsAny:
                - ship
                - train
              then:
                channel: 
                  - [swedesh.se, desh-trans.fr]:
                      service_paid_date:
                        - isBetween: [2018-12-09, '2018-12-10 21:59:59']
                  - tradesh.no:
                      service_paid_date:
                        - isBetween: [2018-12-09, '2018-12-12 21:59:59']
                  - desh-trans.be:
                      service_paid_date:
                        - isBetween: [2018-12-10, '2018-12-14 21:59:59']
                  - [deshtra.co.uk, desh-trans.de]:
                      service_paid_date:
                        - isBetween: [2018-12-09, '2018-12-13 21:59:59']
                decide: '2018-12-24'
            #- else:
            - isBetween: ['AA','zz']
              then:
                channel: 
                  - [swedesh.se, desh-trans.fr]:
                      service_paid_date:
                        - isBetween: [2018-12-09, '2018-12-11 21:59:59']
                  - tradesh.no:
                      service_paid_date:
                        - isBetween: [2018-12-09, '2018-12-13 21:59:59']
                  - [deshtra.co.uk, desh-trans.de, desh-trans.be]:
                      service_paid_date:
                        - isBetween: [2018-12-09, '2018-12-16 21:59:59']
                decide: '2018-12-24'
- transport_type: ship
  channel:
    - contains: inter
      then:
        destination_country:
          - [DE, UK, FR, IT, ES]:
              service_paid_date:
                - isBetween: [2018-12-09, '2018-12-09 21:59:59']
          - [NL, BE]:
              service_paid_date:
                - isBetween: [2018-12-09, '2018-12-13 21:59:59']
          - [NO, SE]:
              service_paid_date:
                - isBetween: [2018-12-01, '2018-12-06 21:59:59']
        decide: '2018-12-21'
              
- transport_type: ship
  channel:
    - contains: export
      then:
        destination_country:
          - [US, CA, AU, NZ]:
              service_paid_date:
                - isBetween: [2018-12-01, '2018-12-02 21:59:59']
              decide: '2018-12-23'
          - [NL, DE, UK]:
              service_paid_date:
                - isBetween: [2018-12-09, '2018-12-14 21:59:59']
          - BE:
              service_paid_date:
                - isBetween: [2018-12-09, '2018-12-17 21:59:59']
          - FR:
              service_paid_date:
                - isBetween: [2018-12-09, '2018-12-10 21:59:59']
          - SE:
              service_paid_date:
                - isBetween: [2018-12-09, '2018-12-09 21:59:59']
          - NO:
              service_paid_date:
                - isBetween: [2018-12-01, '2018-12-06 21:59:59']
        decide: '2018-12-24'
";
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
        [InlineData(
            "{domestic_request: 'yes', service_paid_date: '2018-12-17 14:12:34', channel: 'desh-trans.nl', transport_type: 'ship'}",
            "2018-12-24")]
        [InlineData(
            "{domestic_request: 'yes', service_paid_date: '2018-12-17 14:12:34', channel: 'desh-trans.nl', transport_type: 'train'}",
            "2018-12-24")]
        [InlineData(
            "{domestic_request: 'yes', service_paid_date: '2018-12-18 14:12:34', channel: 'desh-trans.nl', transport_type: 'train'}",
            "2018-12-24")]
        [InlineData(
            "{domestic_request: 'yes', service_paid_date: '2018-12-18 14:12:34', channel: 'desh-trans.nl', transport_type: 'ship'}",
            null)]
        public void Grammar_v010(string contextJson, string expectedDecision)
        {
            ParseExecuteTestRunner.AssertPicksCorrectDecision(Grammar_010, contextJson, expectedDecision);
        }

        [Theory]
        [InlineData(
            "{domestic_request: 'yes', service_paid_date: '2018-12-17 14:12:34', channel: 'desh-trans.nl', transport_type: 'ship'}",
            "2018-12-24")]
        public void Grammar_v020(string contextJson, string expectedDecision)
        {
            ParseExecuteTestRunner.AssertPicksCorrectDecision(Grammar_020, contextJson, expectedDecision);
        }
    }
}
