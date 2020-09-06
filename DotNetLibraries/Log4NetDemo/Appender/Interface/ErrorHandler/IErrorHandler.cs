using System;

namespace Log4NetDemo.Appender.ErrorHandler
{
    /// <summary>
    /// Appenders may delegate their error handling to an <see cref="IErrorHandler" />.
    /// </summary>
    public interface IErrorHandler
    {
        void Error(string message, Exception e, ErrorCode errorCode);

        void Error(string message, Exception e);

        void Error(string message);
    }
}
