using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender
{
    /// <summary>
    /// 批量处理日志的接口
    /// </summary>
    public interface IBulkAppender : IAppender
    {
        void DoAppend(LoggingEvent[] loggingEvents);
    }
}
