using System;
using System.Collections.Generic;
using System.Linq;
using Desh.Execution;
using Desh.Parsing;
using Newtonsoft.Json;
using Xunit;

namespace Desh.Test
{
    public class BasicDeshParsingAndExecution
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

        public class VariableEvaluator : IVariableEvaluator
        {
            private readonly Dictionary<string, string> _variableValues;

            public VariableEvaluator(Dictionary<string, string> variableValues)
            {
                _variableValues = variableValues;
            }

            public string Evaluate(string variable)
            {
                return _variableValues[variable];
            }
        }

        public class OperatorEvaluator : IOperatorEvaluator
        {
            private readonly Dictionary<string, Func<string, string[], bool>> _operators;

            public OperatorEvaluator(Dictionary<string, Func<string, string[], bool>> operators)
            {
                _operators = operators;
            }

            public bool Evaluate(string variableValue, string operatorName, string[] arguments)
            {
                return _operators[operatorName](variableValue, arguments);
            }
        }

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



        public void Picks_correct_decision(string desh, string contextJson, string expectedDecision)
        {
            var parser = new DeshParser();
            
            var ast = parser.Parse(desh);
            var context = JsonConvert.DeserializeObject<Dictionary<string, string>>(contextJson);

            var vars = new VariableEvaluator(context);
            var opDic = new Dictionary<string, Func<string, string[], bool>>
            {
                { ".equals", (varValue, allowedVariants) => allowedVariants.Any(variant => StringComparer.InvariantCultureIgnoreCase.Compare(varValue, variant) == 0) },
                { ".hasLength", (varValue, allowedLengths) => allowedLengths.Any(length => varValue.Length == int.Parse(length)) },
            };
            var ops = new OperatorEvaluator(opDic);
            var engine = new Engine(vars, ops, true);
            var result = engine.Execute(ast);
            if (expectedDecision == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.Equal(expectedDecision, Assert.Single(Assert.IsType<Conclusion>(result).Decisions));
            }
        }
    }
}
