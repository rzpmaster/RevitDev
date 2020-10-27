using System;
using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender.Interface.Evaluator
{
    public class LevelEvaluator : ITriggeringEventEvaluator
    {
        private Level m_threshold;

        public LevelEvaluator() : this(Level.Off)
        {
        }

        public LevelEvaluator(Level threshold)
        {
            if (threshold == null)
            {
                throw new ArgumentNullException("threshold");
            }

            m_threshold = threshold;
        }

        public Level Threshold
        {
            get { return m_threshold; }
            set { m_threshold = value; }
        }

        public bool IsTriggeringEvent(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            return (loggingEvent.Level >= m_threshold);
        }
    }
}
