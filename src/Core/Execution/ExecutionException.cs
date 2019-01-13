using System;
using System.Runtime.Serialization;

namespace Desh.Core.Execution
{
    public class ExecutionException : Exception
    {
        public ExecutionException()
        {
        }

        protected ExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExecutionException(string message) : base(message)
        {
        }

        public ExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
