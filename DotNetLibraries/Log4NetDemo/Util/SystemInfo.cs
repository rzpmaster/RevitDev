using System;
using System.Configuration;

namespace Log4NetDemo.Util
{
    public sealed class SystemInfo
    {
        private const string DEFAULT_NULL_TEXT = "(null)";
        private const string DEFAULT_NOT_AVAILABLE_TEXT = "NOT AVAILABLE";  //无法使用

        private SystemInfo() { }

        static SystemInfo()
        {
            string nullText = DEFAULT_NULL_TEXT;
            string notAvailableText = DEFAULT_NOT_AVAILABLE_TEXT;

            // Look for log4net.NullText in AppSettings
            string nullTextAppSettingsKey = SystemInfo.GetAppSetting("log4net.NullText");
            if (nullTextAppSettingsKey != null && nullTextAppSettingsKey.Length > 0)
            {
                LogLog.Debug(declaringType, "Initializing NullText value to [" + nullTextAppSettingsKey + "].");
                nullText = nullTextAppSettingsKey;
            }

            // Look for log4net.NotAvailableText in AppSettings
            string notAvailableTextAppSettingsKey = SystemInfo.GetAppSetting("log4net.NotAvailableText");
            if (notAvailableTextAppSettingsKey != null && notAvailableTextAppSettingsKey.Length > 0)
            {
                LogLog.Debug(declaringType, "Initializing NotAvailableText value to [" + notAvailableTextAppSettingsKey + "].");
                notAvailableText = notAvailableTextAppSettingsKey;
            }

            s_notAvailableText = notAvailableText;
            s_nullText = nullText;
        }

        #region Public Static Methods

        public static string GetAppSetting(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (Exception ex)
            {
                // If an exception is thrown here then it looks like the config file does not parse correctly.
                LogLog.Error(declaringType, "Exception while reading ConfigurationSettings. Check your .config file is well formed XML.", ex);
            }
            return null;
        }

        #endregion


        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the SystemInfo class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(SystemInfo);

        /// <summary>
        /// Cache the host name for the current machine
        /// </summary>
        private static string s_hostName;

        /// <summary>
        /// Cache the application friendly name
        /// </summary>
        private static string s_appFriendlyName;

        /// <summary>
        /// Text to output when a <c>null</c> is encountered.
        /// </summary>
        private static string s_nullText;

        /// <summary>
        /// Text to output when an unsupported feature is requested.
        /// </summary>
        private static string s_notAvailableText;

        /// <summary>
        /// Start time for the current process.
        /// </summary>
        private static DateTime s_processStartTimeUtc = DateTime.UtcNow;

        #endregion
    }
}
