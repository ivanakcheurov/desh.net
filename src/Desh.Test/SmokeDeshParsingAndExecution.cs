using System.Threading.Tasks;
using Xunit;

namespace Desh.Test
{
    public class SmokeDeshParsingAndExecution
    {
        private const string Desh = @"
someProp:
  - value0 							#pure scalar value
  - [value0a, value0b] 				#scalar (OR) list of values
  - value1:							#value expression tree		
      - anotherProp:
          - anotherValue: 
              decide: decision_A
  - [value2, value3]: 
      decide: decision_B		# OR list value expression	
  - .hasLength: 2
    then:
      - thirdProp:
          - thirdValue: 
              decide: decision_C
decide: YES
";

        [Theory]
        [InlineData(Desh, "{someProp: 'ho', thirdProp: 'thirdValue'}", "decision_C")]
        [InlineData("decide: decision_A", "{}", "decision_A")]
        [InlineData("- decide: decision_A", "{}", "decision_A")]
        //[InlineData("{decide: decision_A, decide: decision_B}", "{}", "decision_A")]
        [InlineData(@"
a: a1
decide: decision_A", "{a: 'a1'}", "decision_A")]
        [InlineData(@"
a: a1
b: b1
decide: decision_A", "{a: 'a1', b: 'b1'}", "decision_A")]
        [InlineData(@"
a: a1
b: b1
decide: decision_A", "{a: 'a1', b: 'b2'}", null)]
        [InlineData(@"
a: a1
b: b1
decide: decision_A", "{a: 'a2', b: 'b1'}", null)]
        [InlineData(@"
- a: a1
  b: b1
  decide: decision_A", "{a: 'a1', b: 'b1'}", "decision_A")]
        [InlineData(@"
- a: a1
  b: b1
  decide: decision_A
- a: a1
  b: b2
  decide: decision_B", "{a: 'a1', b: 'b1'}", "decision_A")]
        [InlineData(@"
- a: a1
  b: b1
  decide: decision_A
- a: a1
  b: b2
  decide: decision_B", "{a: 'a1', b: 'b2'}", "decision_B")]
        [InlineData(@"
- a: a1
  b: b1
  decide: decision_A
- a: a2
  b: Y
  decide: decision_B", "{a: 'a2', b: 'Z'}", null)]
        [InlineData(@"
- b: [b1, b2, b3]
  decide: decision_A", "{b: 'b1'}", "decision_A")]
        [InlineData(@"
- b: [b1, b2, b3]
  decide: decision_A", "{b: 'b2'}", "decision_A")]
        [InlineData(@"
- b: [b1, b2, b3]
  decide: decision_A", "{b: 'b3'}", "decision_A")]
        [InlineData(@"
- b:
    - b1:
        x: y
        decide: decision_B
  decide: decision_A", "{b: 'b1', x: 'y'}", "decision_B")]
        [InlineData(@"
- b:
    - b1:
        x: y
        decide: decision_B
  decide: decision_A", "{b: 'b1', x: 'Z'}", null)]
        [InlineData(@"
- b:
    - [b1, b2]:
        x: y
        decide: decision_B
  decide: decision_A", "{b: 'b2', x: 'y'}", "decision_B")]
        [InlineData(@"
- b:
    - [b1, b2]:
        - x: y
          decide: decision_B
        - x: z
          decide: decision_Z
  decide: decision_A", "{b: 'b2', x: 'y'}", "decision_B")]
        [InlineData(@"
- b:
    - [b1, b2]:
        - x: y
          decide: decision_B
        - x: z
          decide: decision_Z
  decide: decision_A", "{b: 'b1', x: 'z'}", "decision_Z")]
        [InlineData(@"
a: 
  .equals: a1
  decide: decision_A", "{a: 'a1'}", "decision_A")]
        [InlineData(@"
a: 
  .equals: a1
  then:
    decide: decision_A", "{a: 'a1', b: 'b1'}", "decision_A")]
        [InlineData(@"
a: 
  .equals: a1
  then:
    b: b1
    decide: decision_A", "{a: 'a1', b: 'b1'}", "decision_A")]
        [InlineData(@"
a: 
  - .equals: a1
    decide: decision_A", "{a: 'a1'}", "decision_A")]
        [InlineData(@"
a: 
  - .equals: a1
    decide: decision_A
  - .equals: B
    decide: decision_B", "{a: 'a1'}", "decision_A")]
        [InlineData(@"
a: 
  - .equals: a1
    decide: decision_A
  - .equals: B
    decide: decision_B", "{a: 'B'}", "decision_B")]
        [InlineData(@"
a: 
  - .equals: a1
decide: decision_A", "{a: 'a1'}", "decision_A")]
        [InlineData(@"
a: 
  - .equals: a1
    decide: decision_X
decide: decision_A", "{a: 'a1'}", "decision_X")]
        [InlineData(@"
a: 
  - .equals: a1
    decide: decision_X
decide: decision_A", "{a: 'R'}", null)]
        [InlineData(@"
a: 
  - .equals: a1
decide: decision_A", "{a: 'R'}", null)]
        [InlineData(@"
a: 
  - X: decision_A
  - Y: decision_B", "{a: 'X'}", "decision_A")]
        [InlineData(@"
a: 
  - X: decision_A
  - Y: decision_B", "{a: 'Y'}", "decision_B")]
        [InlineData(@"
a: 
  - X: decision_A
  - Y: 
        then: decision_B", "{a: 'Y'}", "decision_B")]
        public async Task Picks_correct_decision(string desh, string contextJson, string expectedDecision)
        {
            await ParseExecuteTestRunner.AssertPicksCorrectDecision(desh, contextJson, expectedDecision);
        }

        //public void OtherTest()
        //{
        //    var parser = new DeshParser();

        //    var parseLogger = new ParseLogger();
        //    var ast = parser.Parse(File.ReadAllText(@"C:\Users\Ivan\Downloads\CMRBD_cutoff_rules.yml"), parseLogger);
        //    var context = JsonConvert.DeserializeObject<Dictionary<string, string>>("{isHomeCountry: 'yes', }");

        //    var vars = new VariableEvaluator(context);
        //    var opDic = new Dictionary<string, Func<string, string[], bool>>
        //    {
        //        { ".equals", (varValue, allowedVariants) => allowedVariants.Any(variant => StringComparer.InvariantCultureIgnoreCase.Compare(varValue, variant) == 0) },
        //        { ".hasLength", (varValue, allowedLengths) => allowedLengths.Any(length => varValue.Length == int.Parse(length)) },
        //    };
        //    var ops = new LambdaOperatorEvaluator(opDic);
        //    var engine = new Engine(vars, ops, true);
        //    var result = engine.Execute(ast);
            
        //    //Assert.Equal(expectedDecision, Assert.Single(Assert.IsType<Conclusion>(result).Decisions));
            
        //}
    }
}
