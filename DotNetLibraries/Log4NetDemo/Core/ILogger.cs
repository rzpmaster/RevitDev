using Log4NetDemo.Core.Data;
using Log4NetDemo.Repository;
using System;

namespace Log4NetDemo.Core
{
    /// <summary>
    /// 具体的一个写某个类型日志的类
    /// </summary>
    /// <para>
    /// 注意，这里的方法不能抛出异常
    /// </para>
    public interface ILogger
    {
        string Name { get; }

        /// <summary>
        /// 通用的所有日志包装器使用的方法
        /// </summary>
        /// <param name="callerStackBoundaryDeclaringType">调用方法的堆栈信息</param>
        /// <param name="level">日志等级</param>
        /// <param name="message">要记录的日志信息</param>
        /// <param name="exception">要记录的异常，可以为空</param>
        void Log(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception);

        /// <summary>
        /// 这是给日志包装器使用的最通用的方法，通过 Logger 记录的顶的日志事件
        /// </summary>
        /// <param name="logEvent">日志事件</param>
        void Log(LoggingEvent logEvent);

        /// <summary>
        /// 某个等级的日志等级是否被允许
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        bool IsEnabledFor(Level level);

        /// <summary>
        /// 日志 Logger实例 的存储库
        /// </summary>
        ILoggerRepository Repository { get; }
    }
}
