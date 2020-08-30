using System;
using System.Text;

namespace Log4NetDemo.Layout.Data.DataFormatter
{
    public class Iso8601DateFormatter : AbsoluteTimeDateFormatter
    {
        override protected void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
        {
            buffer.Append(dateToFormat.Year);

            buffer.Append('-');
            int month = dateToFormat.Month;
            if (month < 10)
            {
                buffer.Append('0');
            }
            buffer.Append(month);
            buffer.Append('-');

            int day = dateToFormat.Day;
            if (day < 10)
            {
                buffer.Append('0');
            }
            buffer.Append(day);
            buffer.Append(' ');

            // Append the 'HH:mm:ss'
            base.FormatDateWithoutMillis(dateToFormat, buffer);
        }
    }
}
