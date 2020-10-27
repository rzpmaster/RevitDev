using System;
using Log4NetDemo.Util;

namespace Log4NetDemo.Appender.ErrorHandler
{
    /// <summary>
    /// 只处理一个错误
    /// </summary>
    /// <remarks>
    /// <para>使用 内部 LogLog 记录错误消息</para>
    /// <para>旨在防止别的引用程序配错误消息淹没</para>
    /// </remarks>
    public class OnlyOnceErrorHandler : IErrorHandler
    {
        public OnlyOnceErrorHandler()
        {
            m_prefix = "";
        }

        public OnlyOnceErrorHandler(string prefix)
        {
            m_prefix = prefix;
        }

        public void Reset()
        {
            m_enabledDateUtc = DateTime.MinValue;
            m_errorCode = ErrorCode.GenericFailure;
            m_exception = null;
            m_message = null;
            m_firstTime = true;
        }

        #region Implementation of IErrorHandler

        /// <summary>
        /// Log an Error
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="errorCode">The internal error code.</param>
        /// <remarks>
        /// <para>
        /// Invokes <see cref="FirstError"/> if and only if this is the first error or the first error after <see cref="Reset"/> has been called.
        /// </para>
        /// </remarks>
        public void Error(string message, Exception e, ErrorCode errorCode)
        {
            if (m_firstTime)
            {
                FirstError(message, e, errorCode);
            }
        }

        /// <summary>
        /// Log the very first error
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="errorCode">The internal error code.</param>
        /// <remarks>
        /// <para>
        /// Sends the error information to <see cref="LogLog"/>'s Error method.
        /// </para>
        /// </remarks>
        public virtual void FirstError(string message, Exception e, ErrorCode errorCode)
        {
            m_enabledDateUtc = DateTime.UtcNow;
            m_errorCode = errorCode;
            m_exception = e;
            m_message = message;

            m_firstTime = false;

            if (LogLog.InternalDebugging && !LogLog.QuietMode)
            {
                LogLog.Error(declaringType, "[" + m_prefix + "] ErrorCode: " + errorCode.ToString() + ". " + message, e);
            }
        }

        /// <summary>
        /// Log an Error
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="e">The exception.</param>
        /// <remarks>
        /// <para>
        /// Invokes <see cref="FirstError"/> if and only if this is the first error or the first error after <see cref="Reset"/> has been called.
        /// </para>
        /// </remarks>
        public void Error(string message, Exception e)
        {
            Error(message, e, ErrorCode.GenericFailure);
        }

        /// <summary>
        /// Log an error
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <remarks>
        /// <para>
        /// Invokes <see cref="FirstError"/> if and only if this is the first error or the first error after <see cref="Reset"/> has been called.
        /// </para>
        /// </remarks>
        public void Error(string message)
        {
            Error(message, null, ErrorCode.GenericFailure);
        }

        #endregion Implementation of IErrorHandler


        #region Public Instance Properties

        /// <summary>
        /// Is error logging enabled
        /// </summary>
        /// <remarks>
        /// <para>
        /// Is error logging enabled. Logging is only enabled for the
        /// first error delivered to the <see cref="OnlyOnceErrorHandler"/>.
        /// </para>
        /// </remarks>
        public bool IsEnabled
        {
            get { return m_firstTime; }
        }

        /// <summary>
        /// The date the first error that trigged this error handler occurred, or <see cref="DateTime.MinValue"/> if it has not been triggered.
        /// </summary>
        public DateTime EnabledDate
        {
            get
            {
                if (m_enabledDateUtc == DateTime.MinValue) return DateTime.MinValue;
                return m_enabledDateUtc.ToLocalTime();
            }
        }

        /// <summary>
        /// The UTC date the first error that trigged this error handler occured, or <see cref="DateTime.MinValue"/> if it has not been triggered.
        /// </summary>
        public DateTime EnabledDateUtc
        {
            get { return m_enabledDateUtc; }
        }

        /// <summary>
        /// The message from the first error that trigged this error handler.
        /// </summary>
        public string ErrorMessage
        {
            get { return m_message; }
        }

        /// <summary>
        /// The exception from the first error that trigged this error handler.
        /// </summary>
        /// <remarks>
        /// May be <see langword="null" />.
        /// </remarks>
        public Exception Exception
        {
            get { return m_exception; }
        }

        /// <summary>
        /// The error code from the first error that trigged this error handler.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="log4net.Core.ErrorCode.GenericFailure"/>
        /// </remarks>
        public ErrorCode ErrorCode
        {
            get { return m_errorCode; }
        }

        #endregion

        #region Private Instance Fields

        /// <summary>
        /// The UTC date the error was recorded.
        /// </summary>
        private DateTime m_enabledDateUtc;

        /// <summary>
        /// Flag to indicate if it is the first error
        /// </summary>
        private bool m_firstTime = true;

        /// <summary>
        /// The message recorded during the first error.
        /// </summary>
        private string m_message = null;

        /// <summary>
        /// The exception recorded during the first error.
        /// </summary>
        private Exception m_exception = null;

        /// <summary>
        /// The error code recorded during the first error.
        /// </summary>
        private ErrorCode m_errorCode = ErrorCode.GenericFailure;

        /// <summary>
        /// String to prefix each message with
        /// </summary>
        private readonly string m_prefix;

        #endregion Private Instance Fields

        private readonly static Type declaringType = typeof(OnlyOnceErrorHandler);
    }
}
