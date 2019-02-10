using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Desh.Execution.Logging
{
    public class Step
    {
        [YamlIgnore]
        public int Number { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeshSpan { get; set; }
        public StepType Type { get; set; }
        public string VariableName { get; set; }
        public string VariableValue { get; set; }
        public string Exception { get; set; }

        public string OperatorName { get; set; }
        public string OperatorVariableValue { get; set; }
        public string[] OperatorArguments { get; set; }
        public bool? OperatorResult { get; set; }

        public string Decision { get; set; }
        public string Result { get; set; }
        public string SourceLocation { get; set; }
    }
}
