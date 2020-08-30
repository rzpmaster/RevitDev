using Log4NetDemo.Core.Data;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal sealed class ThreadPatternConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(loggingEvent.ThreadName);
        }
    }
}
