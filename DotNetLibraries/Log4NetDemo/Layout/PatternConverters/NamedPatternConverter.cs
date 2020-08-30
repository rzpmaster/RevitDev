using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    public abstract class NamedPatternConverter : PatternLayoutConverter, IOptionHandler
    {
        abstract protected string GetFullyQualifiedName(LoggingEvent loggingEvent);

        sealed override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            string name = GetFullyQualifiedName(loggingEvent);
            if (m_precision <= 0 || name == null || name.Length < 2)
            {
                writer.Write(name);
            }
            else
            {
                int len = name.Length;
                string trailingDot = string.Empty;
                if (name.EndsWith(DOT))
                {
                    trailingDot = DOT;
                    name = name.Substring(0, len - 1);
                    len--;
                }

                int end = name.LastIndexOf(DOT);
                for (int i = 1; end > 0 && i < m_precision; i++)
                {
                    end = name.LastIndexOf('.', end - 1);
                }
                if (end == -1)
                {
                    writer.Write(name + trailingDot);
                }
                else
                {
                    writer.Write(name.Substring(end + 1, len - end - 1) + trailingDot);
                }
            }
        }

        #region Implementation of IOptionHandler

        public void ActivateOptions()
        {
            m_precision = 0;

            if (Option != null)
            {
                string optStr = Option.Trim();
                if (optStr.Length > 0)
                {
                    int precisionVal;
                    if (SystemInfo.TryParse(optStr, out precisionVal))
                    {
                        if (precisionVal <= 0)
                        {
                            LogLog.Error(declaringType, "NamedPatternConverter: Precision option (" + optStr + ") isn't a positive integer.");
                        }
                        else
                        {
                            m_precision = precisionVal;
                        }
                    }
                    else
                    {
                        LogLog.Error(declaringType, "NamedPatternConverter: Precision option \"" + optStr + "\" not a decimal integer.");
                    }
                }
            }
        }

        #endregion

        private int m_precision = 0;
        private const string DOT = ".";

        private readonly static Type declaringType = typeof(NamedPatternConverter);
    }
}
