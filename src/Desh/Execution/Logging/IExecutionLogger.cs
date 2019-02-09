using System;
using System.Collections.Generic;
using System.Text;

namespace Desh.Execution.Logging
{
    public interface IExecutionLogger
    {
        void AddStep(Step step);
    }
}
