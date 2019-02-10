using System;
using System.Collections.Generic;
using System.Text;

namespace Desh.Execution.Logging
{
    public enum StepType
    {
        // numbers are specified to serialize all enum values (by default the default values are not serialized): https://github.com/aaubry/YamlDotNet/issues/251
        ExpandVariable = 1,
        EvaluateOperator = 2,
        MakeConclusion = 3,
        MarkAsPositive = 4,
        ReturnExpressionResult = 5,
    }
}
