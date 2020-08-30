using Log4NetDemo.Core.Data;
using Log4NetDemo.Layout;

namespace Log4NetDemo.Appender
{
    public class TraceAppender : AppenderSkeleton
    {
        public TraceAppender()
        {
        }

        public bool ImmediateFlush
        {
            get { return m_immediateFlush; }
            set { m_immediateFlush = value; }
        }

        public PatternLayout Category
        {
            get { return m_category; }
            set { m_category = value; }
        }

        public override bool Flush(int millisecondsTimeout)
        {
            // Nothing to do if ImmediateFlush is true
            if (m_immediateFlush) return true;

            // System.Diagnostics.Trace and System.Diagnostics.Debug are thread-safe, so no need for lock(this).
            System.Diagnostics.Trace.Flush();
            return true;
        }

        #region Override implementation of AppenderSkeleton

        override protected void Append(LoggingEvent loggingEvent)
        {
            //
            // Write the string to the Trace system
            //
            System.Diagnostics.Trace.Write(RenderLoggingEvent(loggingEvent), m_category.Format(loggingEvent));
            //
            // Flush the Trace system if needed
            //
            if (m_immediateFlush)
            {
                System.Diagnostics.Trace.Flush();
            }
        }

        override protected bool RequiresLayout
        {
            get { return true; }
        }

        #endregion

        private bool m_immediateFlush = true;
        private PatternLayout m_category = new PatternLayout("%logger");
    }
}
