using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Util
{
    public sealed class SystemStringFormat
    {
        private readonly IFormatProvider m_provider;
        private readonly string m_format;
        private readonly object[] m_args;

        public SystemStringFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_provider = provider;
            m_format = format;
            m_args = args;
        }

        public override string ToString()
        {
            return StringFormat(m_provider, m_format, m_args);
        }

        #region StringFormat

        private static string StringFormat(IFormatProvider provider, string format, params object[] args)
        {
            try
            {
                // The format is missing, log null value
                if (format == null)
                {
                    return null;
                }

                // The args are missing - should not happen unless we are called explicitly with a null array
                if (args == null)
                {
                    return format;
                }

                // Try to format the string
                return String.Format(provider, format, args);
            }
            catch (Exception ex)
            {
                LogLog.Warn(declaringType, "Exception while rendering format [" + format + "]", ex);
                return StringFormatError(ex, format, args);
            }
        }

        private static string StringFormatError(Exception formatException, string format, object[] args)
        {
            try
            {
                StringBuilder buf = new StringBuilder("<log4net.Error>");

                if (formatException != null)
                {
                    buf.Append("Exception during StringFormat: ").Append(formatException.Message);
                }
                else
                {
                    buf.Append("Exception during StringFormat");
                }
                buf.Append(" <format>").Append(format).Append("</format>");
                buf.Append("<args>");
                RenderArray(args, buf);
                buf.Append("</args>");
                buf.Append("</log4net.Error>");

                return buf.ToString();
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "INTERNAL ERROR during StringFormat error handling", ex);
                return "<log4net.Error>Exception during StringFormat. See Internal Log.</log4net.Error>";
            }
        }

        private static void RenderArray(Array array, StringBuilder buffer)
        {
            if (array == null)
            {
                buffer.Append(SystemInfo.NullText);
            }
            else
            {
                if (array.Rank != 1)
                {
                    buffer.Append(array.ToString());
                }
                else
                {
                    buffer.Append("{");
                    int len = array.Length;

                    if (len > 0)
                    {
                        RenderObject(array.GetValue(0), buffer);
                        for (int i = 1; i < len; i++)
                        {
                            buffer.Append(", ");
                            RenderObject(array.GetValue(i), buffer);
                        }
                    }
                    buffer.Append("}");
                }
            }
        }

        private static void RenderObject(Object obj, StringBuilder buffer)
        {
            if (obj == null)
            {
                buffer.Append(SystemInfo.NullText);
            }
            else
            {
                try
                {
                    buffer.Append(obj);
                }
                catch (Exception ex)
                {
                    buffer.Append("<Exception: ").Append(ex.Message).Append(">");
                }
            }
        }

        #endregion

        private readonly static Type declaringType = typeof(SystemStringFormat);
    }
}
