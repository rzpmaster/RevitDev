using System.IO;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Layout.PatternConverters;

namespace Log4NetDemo.Test.Layout
{
    class TestMessagePatternConverter : PatternLayoutConverter
    {
        /// <summary>
        /// Convert the pattern to the rendered message
        /// </summary>
        /// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
        /// <param name="loggingEvent">the event being logged</param>
        /// <returns>the relevant location information</returns>
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            loggingEvent.WriteRenderedMessage(writer);
        }
    }

    class MessageAsNamePatternConverter : NamedPatternConverter
    {
        protected override string GetFullyQualifiedName(LoggingEvent loggingEvent)
        {
            return loggingEvent.MessageObject.ToString();
        }
    }
}
