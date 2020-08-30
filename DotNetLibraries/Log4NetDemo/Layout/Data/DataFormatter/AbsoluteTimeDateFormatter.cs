using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Log4NetDemo.Layout.Data.DataFormatter
{
    public class AbsoluteTimeDateFormatter : IDateFormatter
    {
        public const string AbsoluteTimeDateFormat = "ABSOLUTE";
        public const string DateAndTimeDateFormat = "DATE";
        public const string Iso8601TimeDateFormat = "ISO8601";

        #region Implementation of IDateFormatter

        virtual public void FormatDate(DateTime dateToFormat, TextWriter writer)
        {
            lock (s_lastTimeStrings)
            {
                // Calculate the current time precise only to the second
                long currentTimeToTheSecond = (dateToFormat.Ticks - (dateToFormat.Ticks % TimeSpan.TicksPerSecond));

                string timeString = null;
                // Compare this time with the stored last time
                // If we are in the same second then append
                // the previously calculated time string
                if (s_lastTimeToTheSecond != currentTimeToTheSecond)
                {
                    s_lastTimeStrings.Clear();
                }
                else
                {
                    timeString = (string)s_lastTimeStrings[GetType()];
                }

                if (timeString == null)
                {
                    // lock so that only one thread can use the buffer and
                    // update the s_lastTimeToTheSecond and s_lastTimeStrings

                    // PERF: Try removing this lock and using a new StringBuilder each time
                    lock (s_lastTimeBuf)
                    {
                        timeString = (string)s_lastTimeStrings[GetType()];

                        if (timeString == null)
                        {
                            // We are in a new second.
                            s_lastTimeBuf.Length = 0;

                            // Calculate the new string for this second
                            FormatDateWithoutMillis(dateToFormat, s_lastTimeBuf);

                            // Render the string buffer to a string
                            timeString = s_lastTimeBuf.ToString();// Store the time as a string (we only have to do this once per second)
                            s_lastTimeStrings[GetType()] = timeString;
                            s_lastTimeToTheSecond = currentTimeToTheSecond;
                        }
                    }
                }
                writer.Write(timeString);

                // Append the current millisecond info
                writer.Write(',');
                int millis = dateToFormat.Millisecond;
                if (millis < 100)
                {
                    writer.Write('0');
                }
                if (millis < 10)
                {
                    writer.Write('0');
                }
                writer.Write(millis);
            }
        }

        #endregion

        virtual protected void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
        {
            int hour = dateToFormat.Hour;
            if (hour < 10)
            {
                buffer.Append('0');
            }
            buffer.Append(hour);
            buffer.Append(':');

            int mins = dateToFormat.Minute;
            if (mins < 10)
            {
                buffer.Append('0');
            }
            buffer.Append(mins);
            buffer.Append(':');

            int secs = dateToFormat.Second;
            if (secs < 10)
            {
                buffer.Append('0');
            }
            buffer.Append(secs);
        }

        private static long s_lastTimeToTheSecond = 0;
        private static StringBuilder s_lastTimeBuf = new StringBuilder();
        private static Hashtable s_lastTimeStrings = new Hashtable();
    }
}
