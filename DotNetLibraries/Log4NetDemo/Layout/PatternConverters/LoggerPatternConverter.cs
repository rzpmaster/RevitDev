using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal sealed class LoggerPatternConverter : NamedPatternConverter
    {
        override protected string GetFullyQualifiedName(LoggingEvent loggingEvent)
        {
            return loggingEvent.LoggerName;
        }
    }
}
