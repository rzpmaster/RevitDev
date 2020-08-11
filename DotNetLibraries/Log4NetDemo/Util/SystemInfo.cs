using System;
using System.Configuration;
using System.Reflection;
using System.Threading;

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

        #region Public Static Properties

        public static string NewLine
        {
            get
            {
                return Environment.NewLine;
            }
        }

        public static string ApplicationBaseDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static string ConfigurationFileLocation
        {
            get
            {
                return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }
        }

        public static string EntryAssemblyLocation
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }
        }

        public static int CurrentThreadId
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }

        public static string HostName
        {
            get
            {
                if (s_hostName == null)
                {

                    // Get the DNS host name of the current machine
                    try
                    {
                        // Lookup the host name
                        s_hostName = System.Net.Dns.GetHostName();
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        LogLog.Debug(declaringType, "Socket exception occurred while getting the dns hostname. Error Ignored.");
                    }
                    catch (System.Security.SecurityException)
                    {
                        // We may get a security exception looking up the hostname
                        // You must have Unrestricted DnsPermission to access resource
                        LogLog.Debug(declaringType, "Security exception occurred while getting the dns hostname. Error Ignored.");
                    }
                    catch (Exception ex)
                    {
                        LogLog.Debug(declaringType, "Some other exception occurred while getting the dns hostname. Error Ignored.", ex);
                    }

                    // Get the NETBIOS machine name of the current machine
                    if (s_hostName == null || s_hostName.Length == 0)
                    {
                        try
                        {
                            s_hostName = Environment.MachineName;
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        catch (System.Security.SecurityException)
                        {
                            // We may get a security exception looking up the machine name
                            // You must have Unrestricted EnvironmentPermission to access resource
                        }
                    }

                    // Couldn't find a value
                    if (s_hostName == null || s_hostName.Length == 0)
                    {
                        s_hostName = s_notAvailableText;
                        LogLog.Debug(declaringType, "Could not determine the hostname. Error Ignored. Empty host name will be used");
                    }
                }
                return s_hostName;
            }
        }

        public static string ApplicationFriendlyName
        {
            get
            {
                if (s_appFriendlyName == null)
                {
                    try
                    {
                        s_appFriendlyName = AppDomain.CurrentDomain.FriendlyName;
                    }
                    catch (System.Security.SecurityException)
                    {
                        // This security exception will occur if the caller does not have 
                        // some undefined set of SecurityPermission flags.
                        LogLog.Debug(declaringType, "Security exception while trying to get current domain friendly name. Error Ignored.");
                    }

                    if (s_appFriendlyName == null || s_appFriendlyName.Length == 0)
                    {
                        try
                        {
                            string assemblyLocation = SystemInfo.EntryAssemblyLocation;
                            s_appFriendlyName = System.IO.Path.GetFileName(assemblyLocation);
                        }
                        catch (System.Security.SecurityException)
                        {
                            // Caller needs path discovery permission
                        }
                    }

                    if (s_appFriendlyName == null || s_appFriendlyName.Length == 0)
                    {
                        s_appFriendlyName = s_notAvailableText;
                    }
                }
                return s_appFriendlyName;
            }
        }

        [Obsolete("Use ProcessStartTimeUtc and convert to local time if needed.")]
        public static DateTime ProcessStartTime
        {
            get { return s_processStartTimeUtc.ToLocalTime(); }
        }

        public static DateTime ProcessStartTimeUtc
        {
            get { return s_processStartTimeUtc; }
        }

        public static string NullText
        {
            get { return s_nullText; }
            set { s_nullText = value; }
        }

        public static string NotAvailableText
        {
            get { return s_notAvailableText; }
            set { s_notAvailableText = value; }
        }

        #endregion

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
