using System;

namespace Log4NetDemo.Appender.ErrorHandler
{
    public interface IErrorHandler
    {
        void Error(string message, Exception e, ErrorCode errorCode);

        void Error(string message, Exception e);

        void Error(string message);
    }
}
