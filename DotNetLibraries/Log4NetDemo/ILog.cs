using Log4NetDemo.Core.Interface;
using System;

namespace Log4NetDemo
{
    /// <summary>
    /// 日志类，具有不同等级的日志方法
    /// </summary>
    /// <para>
    /// 可以以不同方式实现（控制台？日志文件？数据库？）
    /// </para>
    public interface ILog : ILoggerWrapper
    {
        bool IsDebugEnabled { get; }
        void Debug(object message);
        void Debug(object message, Exception exception);
        void DebugFormat(string format, params object[] args);
        void DebugFormat(string format, object arg0);
        void DebugFormat(string format, object arg0, object arg1);
        void DebugFormat(string format, object arg0, object arg1, object arg2);
        void DebugFormat(IFormatProvider provider, string format, params object[] args);

        bool IsInfoEnabled { get; }
        void Info(object message);
        void Info(object message, Exception exception);
        void InfoFormat(string format, params object[] args);
        void InfoFormat(string format, object arg0);
        void InfoFormat(string format, object arg0, object arg1);
        void InfoFormat(string format, object arg0, object arg1, object arg2);
        void InfoFormat(IFormatProvider provider, string format, params object[] args);

        bool IsWarnEnabled { get; }
        void Warn(object message);
        void Warn(object message, Exception exception);
        void WarnFormat(string format, params object[] args);
        void WarnFormat(string format, object arg0);
        void WarnFormat(string format, object arg0, object arg1);
        void WarnFormat(string format, object arg0, object arg1, object arg2);
        void WarnFormat(IFormatProvider provider, string format, params object[] args);

        bool IsErrorEnabled { get; }
        void Error(object message);
        void Error(object message, Exception exception);
        void ErrorFormat(string format, params object[] args);
        void ErrorFormat(string format, object arg0);
        void ErrorFormat(string format, object arg0, object arg1);
        void ErrorFormat(string format, object arg0, object arg1, object arg2);
        void ErrorFormat(IFormatProvider provider, string format, params object[] args);

        bool IsFatalEnabled { get; }
        void Fatal(object message);
        void Fatal(object message, Exception exception);
        void FatalFormat(string format, params object[] args);
        void FatalFormat(string format, object arg0);
        void FatalFormat(string format, object arg0, object arg1);
        void FatalFormat(string format, object arg0, object arg1, object arg2);
        void FatalFormat(IFormatProvider provider, string format, params object[] args);
    }
}
