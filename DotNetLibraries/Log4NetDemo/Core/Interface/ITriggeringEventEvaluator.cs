using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Core.Interface
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>此接口的实现，可以指示 Appender 何时执行 DoAppender 操作</para>
    /// </remarks>
    public interface ITriggeringEventEvaluator
    {
        /// <summary>
        /// 给定 LoggingEvent 是否已经 执行 DoAppender 操作
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        bool IsTriggeringEvent(LoggingEvent loggingEvent);
    }
}
