using Log4NetDemo.Appender.ErrorHandler;
using System;
using System.IO;

namespace Log4NetDemo.Util.TextWriters
{
    public class QuietTextWriter : TextWriterAdapter
    {
        public QuietTextWriter(TextWriter writer, IErrorHandler errorHandler) : base(writer)
        {
            if (errorHandler == null)
            {
                throw new ArgumentNullException("errorHandler");
            }
            ErrorHandler = errorHandler;
        }

        public IErrorHandler ErrorHandler
        {
            get { return m_errorHandler; }
            set
            {
                if (value == null)
                {
                    // This is a programming error on the part of the enclosing appender.
                    throw new ArgumentNullException("value");
                }
                m_errorHandler = value;
            }
        }

        public bool Closed
        {
            get { return m_closed; }
        }

        #region Override Implementation of TextWriter

        public override void Write(char value)
        {
            try
            {
                base.Write(value);
            }
            catch (Exception e)
            {
                m_errorHandler.Error("Failed to write [" + value + "].", e, ErrorCode.WriteFailure);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            try
            {
                base.Write(buffer, index, count);
            }
            catch (Exception e)
            {
                m_errorHandler.Error("Failed to write buffer.", e, ErrorCode.WriteFailure);
            }
        }

        override public void Write(string value)
        {
            try
            {
                base.Write(value);
            }
            catch (Exception e)
            {
                m_errorHandler.Error("Failed to write [" + value + "].", e, ErrorCode.WriteFailure);
            }
        }

        override public void Close()
        {
            m_closed = true;
            base.Close();
        }

        #endregion

        private IErrorHandler m_errorHandler;
        private bool m_closed = false;
    }
}
