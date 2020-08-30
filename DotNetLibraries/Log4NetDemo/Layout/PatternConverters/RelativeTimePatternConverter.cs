using Log4NetDemo.Core.Data;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal sealed class RelativeTimePatternConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(TimeDifferenceInMillis(LoggingEvent.StartTimeUtc, loggingEvent.TimeStampUtc).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        private static long TimeDifferenceInMillis(DateTime start, DateTime end)
        {
            // We must convert all times to UTC before performing any mathematical
            // operations on them. This allows use to take into account discontinuities
            // caused by daylight savings time transitions.
            return (long)(end.ToUniversalTime() - start.ToUniversalTime()).TotalMilliseconds;
        }
    }
}
