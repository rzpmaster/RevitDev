using Log4NetDemo.Appender.ErrorHandler;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using Log4NetDemo.Util.TextWriters;
using System;
using System.IO;

namespace Log4NetDemo.Appender
{
    public class TextWriterAppender : AppenderSkeleton
    {
        public TextWriterAppender()
        {
        }

        public bool ImmediateFlush
        {
            get { return m_immediateFlush; }
            set { m_immediateFlush = value; }
        }

        virtual public TextWriter Writer
        {
            get { return m_qtw; }
            set
            {
                lock (this)
                {
                    Reset();
                    if (value != null)
                    {
                        m_qtw = new QuietTextWriter(value, ErrorHandler);
                        WriteHeader();
                    }
                }
            }
        }

        #region Protected Instance Methods

        virtual protected void WriteFooterAndCloseWriter()
        {
            WriteFooter();
            CloseWriter();
        }

        virtual protected void CloseWriter()
        {
            if (m_qtw != null)
            {
                try
                {
                    m_qtw.Close();
                }
                catch (Exception e)
                {
                    ErrorHandler.Error("Could not close writer [" + m_qtw + "]", e);
                    // do need to invoke an error handler
                    // at this late stage
                }
            }
        }

        virtual protected void Reset()
        {
            WriteFooterAndCloseWriter();
            m_qtw = null;
        }

        virtual protected void WriteFooter()
        {
            if (Layout != null && m_qtw != null && !m_qtw.Closed)
            {
                string f = Layout.Footer;
                if (f != null)
                {
                    m_qtw.Write(f);
                }
            }
        }

        virtual protected void WriteHeader()
        {
            if (Layout != null && m_qtw != null && !m_qtw.Closed)
            {
                string h = Layout.Header;
                if (h != null)
                {
                    m_qtw.Write(h);
                }
            }
        }

        virtual protected void PrepareWriter()
        {
        }

        protected QuietTextWriter QuietWriter
        {
            get { return m_qtw; }
            set { m_qtw = value; }
        }

        #endregion

        #region Override implementation of AppenderSkeleton

        override protected bool PreAppendCheck()
        {
            if (!base.PreAppendCheck())
            {
                return false;
            }

            if (m_qtw == null)
            {
                // Allow subclass to lazily create the writer
                PrepareWriter();

                if (m_qtw == null)
                {
                    ErrorHandler.Error("No output stream or file set for the appender named [" + Name + "].");
                    return false;
                }
            }
            if (m_qtw.Closed)
            {
                ErrorHandler.Error("Output stream for appender named [" + Name + "] has been closed.");
                return false;
            }

            return true;
        }

        override protected void Append(LoggingEvent loggingEvent)
        {
            RenderLoggingEvent(m_qtw, loggingEvent);

            if (m_immediateFlush)
            {
                m_qtw.Flush();
            }
        }

        override protected void Append(LoggingEvent[] loggingEvents)
        {
            foreach (LoggingEvent loggingEvent in loggingEvents)
            {
                RenderLoggingEvent(m_qtw, loggingEvent);
            }

            if (m_immediateFlush)
            {
                m_qtw.Flush();
            }
        }

        override protected void OnClose()
        {
            lock (this)
            {
                Reset();
            }
        }

        override public IErrorHandler ErrorHandler
        {
            get { return base.ErrorHandler; }
            set
            {
                lock (this)
                {
                    if (value == null)
                    {
                        LogLog.Warn(declaringType, "TextWriterAppender: You have tried to set a null error-handler.");
                    }
                    else
                    {
                        base.ErrorHandler = value;
                        if (m_qtw != null)
                        {
                            m_qtw.ErrorHandler = value;
                        }
                    }
                }
            }
        }

        override protected bool RequiresLayout
        {
            get { return true; }
        }

        #endregion

        #region implementation of IFlushable

        public override bool Flush(int millisecondsTimeout)
        {
            // Nothing to do if ImmediateFlush is true
            if (m_immediateFlush) return true;

            // lock(this) will block any Appends while the buffer is flushed.
            lock (this)
            {
                m_qtw.Flush();
            }

            return true;
        }

        #endregion

        private QuietTextWriter m_qtw;
        private bool m_immediateFlush = true;

        private readonly static Type declaringType = typeof(TextWriterAppender);
    }
}
