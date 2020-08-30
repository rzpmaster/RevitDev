using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal class UtcDatePatternConverter : DatePatternConverter
    {
        override protected void Convert(TextWriter writer, object state)
        {
            try
            {
                m_dateFormatter.FormatDate(DateTime.UtcNow, writer);
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Error occurred while converting date.", ex);
            }
        }

        private readonly static Type declaringType = typeof(UtcDatePatternConverter);
    }
}
