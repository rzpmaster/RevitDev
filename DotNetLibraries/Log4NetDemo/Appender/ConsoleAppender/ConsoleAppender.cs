using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Appender
{
    public class ConsoleAppender : AppenderSkeleton
    {
        public ConsoleAppender()
        {
        }

        virtual public string Target
        {
            get { return m_writeToErrorStream ? ConsoleError : ConsoleOut; }
            set
            {
                string v = value.Trim();

                if (SystemInfo.EqualsIgnoringCase(ConsoleError, v))
                {
                    m_writeToErrorStream = true;
                }
                else
                {
                    m_writeToErrorStream = false;
                }
            }
        }

        #region Override implementation of AppenderSkeleton

        override protected void Append(LoggingEvent loggingEvent)
        {
            if (m_writeToErrorStream)
            {
                // Write to the error stream
                Console.Error.Write(RenderLoggingEvent(loggingEvent));
            }
            else
            {
                // Write to the output stream
                Console.Write(RenderLoggingEvent(loggingEvent));
            }
        }

        override protected bool RequiresLayout
        {
            get { return true; }
        }

        #endregion

        public const string ConsoleOut = "Console.Out";
        public const string ConsoleError = "Console.Error";

        private bool m_writeToErrorStream = false;
    }
}
