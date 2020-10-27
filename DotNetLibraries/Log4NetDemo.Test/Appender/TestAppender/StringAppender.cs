using System.Text;
using Log4NetDemo.Appender;
using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Test.Appender.TestAppender
{
    class StringAppender : AppenderSkeleton
    {
        private StringBuilder m_buf = new StringBuilder();

        public StringAppender()
        {
        }

        public string GetString()
        {
            return m_buf.ToString();
        }

        public void Reset()
        {
            m_buf.Length = 0;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            m_buf.Append(RenderLoggingEvent(loggingEvent));
        }

        protected override bool RequiresLayout
        {
            get { return true; }
        }
    }
}
