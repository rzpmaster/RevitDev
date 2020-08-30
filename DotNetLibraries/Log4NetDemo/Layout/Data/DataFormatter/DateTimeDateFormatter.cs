using System;
using System.Globalization;
using System.Text;

namespace Log4NetDemo.Layout.Data.DataFormatter
{
    public class DateTimeDateFormatter : AbsoluteTimeDateFormatter
    {
        public DateTimeDateFormatter()
        {
            m_dateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
        }

        override protected void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
        {
            int day = dateToFormat.Day;
            if (day < 10)
            {
                buffer.Append('0');
            }
            buffer.Append(day);
            buffer.Append(' ');

            buffer.Append(m_dateTimeFormatInfo.GetAbbreviatedMonthName(dateToFormat.Month));
            buffer.Append(' ');

            buffer.Append(dateToFormat.Year);
            buffer.Append(' ');

            // Append the 'HH:mm:ss'
            base.FormatDateWithoutMillis(dateToFormat, buffer);
        }

        private readonly DateTimeFormatInfo m_dateTimeFormatInfo;
    }
}
