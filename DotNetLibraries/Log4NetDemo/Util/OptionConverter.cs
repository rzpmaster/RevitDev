using System;

namespace Log4NetDemo.Util
{
    public sealed class OptionConverter
    {
        private OptionConverter() { }

        public static bool ToBoolean(string argValue, bool defaultValue)
        {
            if (argValue != null && argValue.Length > 0)
            {
                try
                {
                    return bool.Parse(argValue);
                }
                catch (Exception e)
                {
                    LogLog.Error(declaringType, "[" + argValue + "] is not in proper bool form.", e);
                }
            }
            return defaultValue;
        }

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the OptionConverter class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(OptionConverter);

        private const string DELIM_START = "${";
        private const char DELIM_STOP = '}';
        private const int DELIM_START_LEN = 2;
        private const int DELIM_STOP_LEN = 1;

        #endregion Private Static Fields
    }
}
