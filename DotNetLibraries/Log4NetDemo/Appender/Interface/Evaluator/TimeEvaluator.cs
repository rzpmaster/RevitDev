using System;
using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender.Interface.Evaluator
{
    public class TimeEvaluator : ITriggeringEventEvaluator
    {
        private int m_interval;

        private DateTime m_lastTimeUtc;

        const int DEFAULT_INTERVAL = 0;

        public TimeEvaluator()
            : this(DEFAULT_INTERVAL)
        {
        }

        public TimeEvaluator(int interval)
        {
            m_interval = interval;
            m_lastTimeUtc = DateTime.UtcNow;
        }

        public int Interval
        {
            get { return m_interval; }
            set { m_interval = value; }
        }

        public bool IsTriggeringEvent(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            // disable the evaluator if threshold is zero
            if (m_interval == 0) return false;

            lock (this) // avoid triggering multiple times
            {
                TimeSpan passed = DateTime.UtcNow.Subtract(m_lastTimeUtc);

                if (passed.TotalSeconds > m_interval)
                {
                    m_lastTimeUtc = DateTime.UtcNow;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
