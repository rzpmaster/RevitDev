using Log4NetDemo.Appender;
using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Test.Appender.TestAppender
{
    class CountingAppender : AppenderSkeleton
    {
        public CountingAppender()
        {
            m_counter = 0;
        }

        public int Counter
        {
            get { return m_counter; }
        }

        public void ResetCounter()
        {
            m_counter = 0;
        }

        protected override void Append(LoggingEvent logEvent)
        {
            m_counter++;
        }

        private int m_counter;
    }
}
