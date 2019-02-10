using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Desh.Test
{
    public class TestDocumentationExample
    {
        [Fact]
        public void TestVisaExample()
        {
            var desh = @"
nationality:
  - NL:
      destination:
        - US: electronic_online_visa
        - UA:
            days_of_stay:
              - number_greater_than: 90
                decide: paper_visa
              - else: no_visa
  - UA:
      destination:
        - US: paper_visa
        - NL:
            biometric_passport:
              - yes: no_visa
              - no: paper_visa
";
            var customer =
                new Dictionary<string, Func<string>>
                {
                    { "nationality", () => "NL" },
                    { "destination", () => "US" }
                };
            var facade = new DeshFacade();
            var visaDecision = facade.ParseAndMakeStringDecision(desh, customer);
            Assert.Equal("electronic_online_visa", visaDecision);
        }
    }
}
