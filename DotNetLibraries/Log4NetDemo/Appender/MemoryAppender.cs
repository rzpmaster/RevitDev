using System.Collections;
using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender
{
    class MemoryAppender : AppenderSkeleton
    {
        public MemoryAppender() : base()
        {
            m_eventsList = new ArrayList();
        }

        virtual public LoggingEvent[] GetEvents()
        {
            lock (m_eventsList.SyncRoot)
            {
                return (LoggingEvent[])m_eventsList.ToArray(typeof(LoggingEvent));
            }
        }

        virtual public FixFlags Fix
        {
            get { return m_fixFlags; }
            set { m_fixFlags = value; }
        }

        #region Override implementation of AppenderSkeleton

        override protected void Append(LoggingEvent loggingEvent)
        {
            // Because we are caching the LoggingEvent beyond the
            // lifetime of the Append() method we must fix any
            // volatile data in the event.
            loggingEvent.Fix = this.Fix;

            lock (m_eventsList.SyncRoot)
            {
                m_eventsList.Add(loggingEvent);
            }
        }

        #endregion

        virtual public void Clear()
        {
            lock (m_eventsList.SyncRoot)
            {
                m_eventsList.Clear();
            }
        }

        virtual public LoggingEvent[] PopAllEvents()
        {
            lock (m_eventsList.SyncRoot)
            {
                LoggingEvent[] tmp = (LoggingEvent[])m_eventsList.ToArray(typeof(LoggingEvent));
                m_eventsList.Clear();
                return tmp;
            }
        }

        protected ArrayList m_eventsList;
        protected FixFlags m_fixFlags = FixFlags.All;
    }
}
