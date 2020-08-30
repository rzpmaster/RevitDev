using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal class UtcDatePatternConverter : DatePatternConverter
    {
        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            try
            {
                m_dateFormatter.FormatDate(loggingEvent.TimeStampUtc, writer);
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Error occurred while converting date.", ex);
            }
        }

        private readonly static Type declaringType = typeof(UtcDatePatternConverter);
    }
}
