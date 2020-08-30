using Log4NetDemo.Core.Data;
using Log4NetDemo.Layout;

namespace Log4NetDemo.Appender
{
    /// <summary>
    /// Appends log events to the <see cref="System.Diagnostics.Debug"/> system.
    /// </summary>
    public class DebugAppender : AppenderSkeleton
    {
        public DebugAppender()
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

            // System.Diagnostics.Debug is thread-safe, so no need for lock(this).
            System.Diagnostics.Debug.Flush();

            return true;
        }

        #region Override implementation of AppenderSkeleton

        override protected void Append(LoggingEvent loggingEvent)
        {
            //
            // Write the string to the Debug system
            //
            if (m_category == null)
            {
                System.Diagnostics.Debug.Write(RenderLoggingEvent(loggingEvent));
            }
            else
            {
                string category = m_category.Format(loggingEvent);
                if (string.IsNullOrEmpty(category))
                {
                    System.Diagnostics.Debug.Write(RenderLoggingEvent(loggingEvent));
                }
                else
                {
                    System.Diagnostics.Debug.Write(RenderLoggingEvent(loggingEvent), category);
                }
            }
            //
            // Flush the Debug system if needed
            //
            if (m_immediateFlush)
            {
                System.Diagnostics.Debug.Flush();
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
