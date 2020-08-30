using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal sealed class TypeNamePatternConverter : NamedPatternConverter
    {
        override protected string GetFullyQualifiedName(LoggingEvent loggingEvent)
        {
            return loggingEvent.LocationInformation.ClassName;
        }
    }
}
