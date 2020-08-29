using System;
using System.Collections;
using System.Configuration;
using System.IO;
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

        /// <summary>
        /// 获取配置文件中的信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 返回程序集的位置信息
        /// </summary>
        /// <param name="myAssembly"></param>
        /// <returns></returns>
        public static string AssemblyLocationInfo(Assembly myAssembly)
        {
            if (myAssembly.GlobalAssemblyCache)
            {
                return "Global Assembly Cache";
            }
            else
            {
                try
                {
                    if (myAssembly.IsDynamic)
                    {
                        return "Dynamic Assembly";
                    }
                    else
                    {
                        // This call requires FileIOPermission for access to the path if we don't have permission then we just ignore it and carry on.
                        // 需要相关权限
                        return myAssembly.Location;
                    }
                }
                catch (NotSupportedException)
                {
                    // The location information may be unavailable for dynamic assemblies and a NotSupportedException is thrown in those cases. See: http://msdn.microsoft.com/de-de/library/system.reflection.assembly.location.aspx
                    // 位置信息可能对动态程序集不可用，在这些情况下会引发NotSupportedException
                    return "Dynamic Assembly";
                }
                catch (TargetInvocationException ex)
                {
                    return "Location Detect Failed (" + ex.Message + ")";
                }
                catch (ArgumentException ex)
                {
                    return "Location Detect Failed (" + ex.Message + ")";
                }
                catch (System.Security.SecurityException)
                {
                    return "Location Permission Denied";
                }
            }
        }

        public static string AssemblyShortName(Assembly myAssembly)
        {
            string name = myAssembly.FullName;
            int offset = name.IndexOf(',');
            if (offset > 0)
            {
                name = name.Substring(0, offset);
            }
            return name.Trim();

            // TODO: Do we need to unescape the assembly name string? 
            // Doc says '\' is an escape char but has this already been 
            // done by the string loader?
        }

        public static string AssemblyFileName(Assembly myAssembly)
        {
            return System.IO.Path.GetFileName(myAssembly.Location);
        }

        public static string AssemblyQualifiedName(Type type)
        {
            return type.FullName + ", " + type.Assembly.FullName;
        }

        public static Type GetTypeFromString(string typeName, bool throwOnError, bool ignoreCase)
        {
            return GetTypeFromString(Assembly.GetCallingAssembly(), typeName, throwOnError, ignoreCase);
        }

        public static Type GetTypeFromString(Type relativeType, string typeName, bool throwOnError, bool ignoreCase)
        {
            return GetTypeFromString(relativeType.Assembly, typeName, throwOnError, ignoreCase);
        }

        public static Type GetTypeFromString(Assembly relativeAssembly, string typeName, bool throwOnError, bool ignoreCase)
        {
            // Check if the type name specifies the assembly name
            if (typeName.IndexOf(',') == -1)
            {
                //LogLog.Debug(declaringType, "SystemInfo: Loading type ["+typeName+"] from assembly ["+relativeAssembly.FullName+"]");

                // Attempt to lookup the type from the relativeAssembly
                Type type = relativeAssembly.GetType(typeName, false, ignoreCase);
                if (type != null)
                {
                    // Found type in relative assembly
                    //LogLog.Debug(declaringType, "SystemInfo: Loaded type ["+typeName+"] from assembly ["+relativeAssembly.FullName+"]");
                    return type;
                }

                Assembly[] loadedAssemblies = null;
                try
                {
                    loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
                catch (System.Security.SecurityException)
                {
                    // Insufficient permissions to get the list of loaded assemblies
                }

                if (loadedAssemblies != null)
                {
                    Type fallback = null;
                    // Search the loaded assemblies for the type
                    foreach (Assembly assembly in loadedAssemblies)
                    {
                        Type t = assembly.GetType(typeName, false, ignoreCase);
                        if (t != null)
                        {
                            // Found type in loaded assembly
                            LogLog.Debug(declaringType, "Loaded type [" + typeName + "] from assembly [" + assembly.FullName + "] by searching loaded assemblies.");
                            if (assembly.GlobalAssemblyCache)
                            {
                                fallback = t;
                            }
                            else
                            {
                                return t;
                            }
                        }
                    }
                    if (fallback != null)
                    {
                        return fallback;
                    }
                }

                // Didn't find the type
                if (throwOnError)
                {
                    throw new TypeLoadException("Could not load type [" + typeName + "]. Tried assembly [" + relativeAssembly.FullName + "] and all loaded assemblies");
                }
                return null;
            }
            else
            {
                // Includes explicit assembly name
                //LogLog.Debug(declaringType, "SystemInfo: Loading type ["+typeName+"] from global Type");
                return Type.GetType(typeName, throwOnError, ignoreCase);
            }
        }

        public static string ConvertToFullPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            string baseDirectory = "";
            try
            {
                string applicationBaseDirectory = SystemInfo.ApplicationBaseDirectory;
                if (applicationBaseDirectory != null)
                {
                    // applicationBaseDirectory may be a URI not a local file path
                    Uri applicationBaseDirectoryUri = new Uri(applicationBaseDirectory);
                    if (applicationBaseDirectoryUri.IsFile)
                    {
                        baseDirectory = applicationBaseDirectoryUri.LocalPath;
                    }
                }
            }
            catch
            {
                // Ignore URI exceptions & SecurityExceptions from SystemInfo.ApplicationBaseDirectory
            }

            if (baseDirectory != null && baseDirectory.Length > 0)
            {
                // Note that Path.Combine will return the second path if it is rooted
                return Path.GetFullPath(Path.Combine(baseDirectory, path));
            }
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// 创建一个初始容量的 不区分大小写 的哈希表
        /// </summary>
        /// <returns></returns>
        public static Hashtable CreateCaseInsensitiveHashtable()
        {
            return new Hashtable(StringComparer.OrdinalIgnoreCase);
        }

        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string parameterName, object actualValue, string message)
        {
            return new ArgumentOutOfRangeException(parameterName, actualValue, message);
        }

        public static Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        public static Boolean EqualsIgnoringCase(String a, String b)
        {
            return String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryParse(string s, out int val)
        {
            // Initialise out param
            val = 0;

            try
            {
                double doubleVal;
                if (Double.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out doubleVal))
                {
                    val = Convert.ToInt32(doubleVal);
                    return true;
                }
            }
            catch
            {
                // Ignore exception, just return false
            }

            return false;
        }

        public static bool TryParse(string s, out long val)
        {
            // Initialise out param
            val = 0;

            try
            {
                double doubleVal;
                if (Double.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out doubleVal))
                {
                    val = Convert.ToInt64(doubleVal);
                    return true;
                }
            }
            catch
            {
                // Ignore exception, just return false
            }

            return false;
        }

        public static bool TryParse(string s, out short val)
        {
            // Initialise out param
            val = 0;

            try
            {
                double doubleVal;
                if (Double.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out doubleVal))
                {
                    val = Convert.ToInt16(doubleVal);
                    return true;
                }
            }
            catch
            {
                // Ignore exception, just return false
            }

            return false;
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
