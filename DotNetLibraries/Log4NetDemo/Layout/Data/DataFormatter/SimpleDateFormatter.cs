using System;
using System.IO;

namespace Log4NetDemo.Layout.Data.DataFormatter
{
    public class SimpleDateFormatter : IDateFormatter
    {
        public SimpleDateFormatter(string format)
        {
            m_formatString = format;
        }

        virtual public void FormatDate(DateTime dateToFormat, TextWriter writer)
        {
            writer.Write(dateToFormat.ToString(m_formatString, System.Globalization.DateTimeFormatInfo.InvariantInfo));
        }

        private readonly string m_formatString;
    }
}
