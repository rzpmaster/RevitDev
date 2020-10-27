using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender.Interface.Evaluator
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface ITriggeringEventEvaluator
    {
        bool IsTriggeringEvent(LoggingEvent loggingEvent);
    }
}
