﻿using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Layout.Data.DataFormatter;
using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal class DatePatternConverter : PatternLayoutConverter, IOptionHandler
    {
        protected IDateFormatter m_dateFormatter;

        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            try
            {
                m_dateFormatter.FormatDate(loggingEvent.TimeStamp, writer);
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Error occurred while converting date.", ex);
            }
        }

        #region Implementation of IOptionHandler

        public void ActivateOptions()
        {
            string dateFormatStr = Option;
            if (dateFormatStr == null)
            {
                dateFormatStr = AbsoluteTimeDateFormatter.Iso8601TimeDateFormat;
            }

            if (SystemInfo.EqualsIgnoringCase(dateFormatStr, AbsoluteTimeDateFormatter.Iso8601TimeDateFormat))
            {
                m_dateFormatter = new Iso8601DateFormatter();
            }
            else if (SystemInfo.EqualsIgnoringCase(dateFormatStr, AbsoluteTimeDateFormatter.AbsoluteTimeDateFormat))
            {
                m_dateFormatter = new AbsoluteTimeDateFormatter();
            }
            else if (SystemInfo.EqualsIgnoringCase(dateFormatStr, AbsoluteTimeDateFormatter.DateAndTimeDateFormat))
            {
                m_dateFormatter = new DateTimeDateFormatter();
            }
            else
            {
                try
                {
                    m_dateFormatter = new SimpleDateFormatter(dateFormatStr);
                }
                catch (Exception e)
                {
                    LogLog.Error(declaringType, "Could not instantiate SimpleDateFormatter with [" + dateFormatStr + "]", e);
                    m_dateFormatter = new Iso8601DateFormatter();
                }
            }
        }

        #endregion

        private readonly static Type declaringType = typeof(DatePatternConverter);
    }
}
