using Log4NetDemo.Core.Data;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    public abstract class PatternLayoutConverter : PatternConverter
    {
        protected PatternLayoutConverter()
        {
        }

        abstract protected void Convert(TextWriter writer, LoggingEvent loggingEvent);

        virtual public bool IgnoresException
        {
            get { return m_ignoresException; }
            set { m_ignoresException = value; }
        }

        override protected void Convert(TextWriter writer, object state)
        {
            LoggingEvent loggingEvent = state as LoggingEvent;
            if (loggingEvent != null)
            {
                Convert(writer, loggingEvent);
            }
            else
            {
                throw new ArgumentException("state must be of type [" + typeof(LoggingEvent).FullName + "]", "state");
            }
        }

        private bool m_ignoresException = true;
    }
}
