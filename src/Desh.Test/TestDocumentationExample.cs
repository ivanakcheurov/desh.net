using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Desh.Test
{
    public class TestDocumentationExample
    {
        [Fact]
        public async Task TestVisaExample()
        {
            // ReSharper disable Xunit.XunitTestWithConsoleOutput
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
                new Dictionary<string, Func<Task<string>>>
                {
                    { "nationality", () => Task.FromResult("NL") },
                    { "destination", () => Task.FromResult("US") }
                };
            var facade = new DeshFacade();
            var visaDecision = await facade.ParseAndMakeStringDecision(desh, customer);
            Console.WriteLine($"Visa requirement: {visaDecision}");
            Assert.Equal("electronic_online_visa", visaDecision);
        }
    }
}
