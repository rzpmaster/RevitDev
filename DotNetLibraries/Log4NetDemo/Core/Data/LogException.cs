using System;
using System.Runtime.Serialization;

namespace Log4NetDemo.Core.Data
{
    public class LogException : ApplicationException
    {
        public LogException()
        {
        }

        public LogException(String message) : base(message)
        {
        }

        public LogException(String message, Exception innerException) : base(message, innerException)
        {
        }

        protected LogException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
