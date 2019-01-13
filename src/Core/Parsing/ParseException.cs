using System;
using System.Runtime.Serialization;

namespace Desh.Core.Parsing
{
    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        protected ParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
