using Log4NetDemo.Appender.ErrorHandler;
using Log4NetDemo.Appender.LockingModels;
using Log4NetDemo.Context;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using Log4NetDemo.Util.TextWriters;
using System;
using System.IO;
using System.Text;

namespace Log4NetDemo.Appender
{
    public class FileAppender : TextWriterAppender
    {
        public FileAppender()
        {
        }

        virtual public string File
        {
            get { return m_fileName; }
            set { m_fileName = value; }
        }

        public bool AppendToFile
        {
            get { return m_appendToFile; }
            set { m_appendToFile = value; }
        }

        public Encoding Encoding
        {
            get { return m_encoding; }
            set { m_encoding = value; }
        }

        public SecurityContext SecurityContext
        {
            get { return m_securityContext; }
            set { m_securityContext = value; }
        }

        public LockingModelBase LockingModel
        {
            get { return m_lockingModel; }
            set { m_lockingModel = value; }
        }

        #region Override implementation of TextWriterAppender

        override protected void Reset()
        {
            base.Reset();
            m_fileName = null;
        }

        override protected void OnClose()
        {
            base.OnClose();
            m_lockingModel.OnClose();
        }

        override protected void PrepareWriter()
        {
            SafeOpenFile(m_fileName, m_appendToFile);
        }

        override protected void Append(LoggingEvent loggingEvent)
        {
            if (m_stream.AcquireLock())
            {
                try
                {
                    base.Append(loggingEvent);
                }
                finally
                {
                    m_stream.ReleaseLock();
                }
            }
        }

        override protected void Append(LoggingEvent[] loggingEvents)
        {
            if (m_stream.AcquireLock())
            {
                try
                {
                    base.Append(loggingEvents);
                }
                finally
                {
                    m_stream.ReleaseLock();
                }
            }
        }

        protected override void WriteFooter()
        {
            if (m_stream != null)
            {
                //WriteFooter can be called even before a file is opened
                m_stream.AcquireLock();
                try
                {
                    base.WriteFooter();
                }
                finally
                {
                    m_stream.ReleaseLock();
                }
            }
        }

        protected override void WriteHeader()
        {
            if (m_stream != null)
            {
                if (m_stream.AcquireLock())
                {
                    try
                    {
                        base.WriteHeader();
                    }
                    finally
                    {
                        m_stream.ReleaseLock();
                    }
                }
            }
        }

        protected override void CloseWriter()
        {
            if (m_stream != null)
            {
                m_stream.AcquireLock();
                try
                {
                    base.CloseWriter();
                }
                finally
                {
                    m_stream.ReleaseLock();
                }
            }
        }

        #endregion

        #region Override implementation of IOptionHandler

        override public void ActivateOptions()
        {
            base.ActivateOptions();

            if (m_securityContext == null)
            {
                m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
            }

            if (m_lockingModel == null)
            {
                m_lockingModel = new ExclusiveLock();
            }

            m_lockingModel.CurrentAppender = this;
            m_lockingModel.ActivateOptions();

            if (m_fileName != null)
            {
                using (SecurityContext.Impersonate(this))
                {
                    m_fileName = ConvertToFullPath(m_fileName.Trim());
                }
                SafeOpenFile(m_fileName, m_appendToFile);
            }
            else
            {
                LogLog.Warn(declaringType, "FileAppender: File option not set for appender [" + Name + "].");
                LogLog.Warn(declaringType, "FileAppender: Are you using FileAppender instead of ConsoleAppender?");
            }
        }

        #endregion

        #region Protected Instance Methods

        protected void CloseFile()
        {
            WriteFooterAndCloseWriter();
        }

        virtual protected void SafeOpenFile(string fileName, bool append)
        {
            try
            {
                OpenFile(fileName, append);
            }
            catch (Exception e)
            {
                ErrorHandler.Error("OpenFile(" + fileName + "," + append + ") call failed.", e, ErrorCode.FileOpenFailure);
            }
        }

        virtual protected void OpenFile(string fileName, bool append)
        {
            if (LogLog.IsErrorEnabled)
            {
                // Internal check that the fileName passed in is a rooted path
                bool isPathRooted = false;
                using (SecurityContext.Impersonate(this))
                {
                    isPathRooted = Path.IsPathRooted(fileName);
                }
                if (!isPathRooted)
                {
                    LogLog.Error(declaringType, "INTERNAL ERROR. OpenFile(" + fileName + "): File name is not fully qualified.");
                }
            }

            lock (this)
            {
                Reset();

                LogLog.Debug(declaringType, "Opening file for writing [" + fileName + "] append [" + append + "]");

                // Save these for later, allowing retries if file open fails
                m_fileName = fileName;
                m_appendToFile = append;

                LockingModel.CurrentAppender = this;
                LockingModel.OpenFile(fileName, append, m_encoding);
                m_stream = new LockingStream(LockingModel);

                if (m_stream != null)
                {
                    m_stream.AcquireLock();
                    try
                    {
                        SetQWForFiles(m_stream);
                    }
                    finally
                    {
                        m_stream.ReleaseLock();
                    }
                }

                WriteHeader();
            }
        }

        virtual protected void SetQWForFiles(Stream fileStream)
        {
            SetQWForFiles(new StreamWriter(fileStream, m_encoding));
        }

        virtual protected void SetQWForFiles(TextWriter writer)
        {
            QuietWriter = new QuietTextWriter(writer, ErrorHandler);
        }

        protected static string ConvertToFullPath(string path)
        {
            return SystemInfo.ConvertToFullPath(path);
        }

        #endregion

        private bool m_appendToFile = true;
        private string m_fileName = null;
        private Encoding m_encoding = Encoding.GetEncoding(0);
        private SecurityContext m_securityContext;
        private LockingStream m_stream = null;
        private LockingModelBase m_lockingModel = new ExclusiveLock();

        private readonly static Type declaringType = typeof(FileAppender);
    }
}
