using Log4NetDemo.Context;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Core.Data
{
    public struct LoggingEventData
    {
        public string LoggerName;
        public Level Level;
        public string Message;
        public string ThreadName;

        [Obsolete("请使用 TimeStampUtc")]
        public DateTime TimeStamp;

#pragma warning disable 618
        private DateTime _timeStampUtc;
        public DateTime TimeStampUtc
        {
            get
            {
                if (TimeStamp != default(DateTime) &&
                    _timeStampUtc == default(DateTime))
                {
                    // TimeStamp field has been set explicitly but TimeStampUtc hasn't
                    // => use TimeStamp
                    return TimeStamp.ToUniversalTime();
                }
                return _timeStampUtc;
            }
            set
            {
                _timeStampUtc = value;
                // For backwards compatibility
                TimeStamp = _timeStampUtc.ToLocalTime();
            }
        }
#pragma warning restore 618

        public LocationInfo LocationInfo;
        public string UserName;
        public string Identity;
        public string ExceptionString;
        public string Domain;
        public PropertiesDictionary Properties;
    }

    [Flags]
    public enum FixFlags
    {
        /// <summary>
        /// Fix the MDC
        /// </summary>
        [Obsolete("Replaced by composite Properties")]
        Mdc = 0x01,

        /// <summary>
        /// Fix the NDC
        /// </summary>
        Ndc = 0x02,

        /// <summary>
        /// Fix the rendered message
        /// </summary>
        Message = 0x04,

        /// <summary>
        /// Fix the thread name
        /// </summary>
        ThreadName = 0x08,

        /// <summary>
        /// Fix the callers location information
        /// </summary>
        /// <remarks>
        /// CAUTION: Very slow to generate
        /// </remarks>
        LocationInfo = 0x10,

        /// <summary>
        /// Fix the callers windows user name
        /// </summary>
        /// <remarks>
        /// CAUTION: Slow to generate
        /// </remarks>
        UserName = 0x20,

        /// <summary>
        /// Fix the domain friendly name
        /// </summary>
        Domain = 0x40,

        /// <summary>
        /// Fix the callers principal name
        /// </summary>
        /// <remarks>
        /// CAUTION: May be slow to generate
        /// 注意：callers 地址 会很慢
        /// </remarks>
        Identity = 0x80,

        /// <summary>
        /// Fix the exception text
        /// </summary>
        Exception = 0x100,

        /// <summary>
        /// Fix the event properties. Active properties must implement <see cref="IFixingRequired"/> in order to be eligible for fixing.
        /// </summary>
        Properties = 0x200,

        /// <summary>
        /// No fields fixed
        /// </summary>
        None = 0x0,

        /// <summary>
        /// All fields fixed
        /// </summary>
        All = 0xFFFFFFF,

        /// <summary>
        /// Partial fields fixed
        /// </summary>
        /// <remarks>
        /// <para>
        /// This set of partial fields gives good performance. The following fields are fixed:
        /// </para>
        /// <list type="bullet">
        /// <item><description><see cref="FixFlags.Message"/></description></item>
        /// <item><description><see cref="FixFlags.ThreadName"/></description></item>
        /// <item><description><see cref="FixFlags.Exception"/></description></item>
        /// <item><description><see cref="FixFlags.Domain"/></description></item>
        /// <item><description><see cref="FixFlags.Properties"/></description></item>
        /// </list>
        /// </remarks>
        Partial = Message | ThreadName | Exception | Domain | Properties,
    }

    [Serializable]
    public class LoggingEvent : ISerializable
    {
        #region Constructors

        public LoggingEvent(Type callerStackBoundaryDeclaringType, ILoggerRepository repository, string loggerName, Level level, object message, Exception exception)
        {
            m_callerStackBoundaryDeclaringType = callerStackBoundaryDeclaringType;
            m_message = message;
            m_repository = repository;
            m_thrownException = exception;

            m_data.LoggerName = loggerName;
            m_data.Level = level;

            // Store the event creation time
            m_data.TimeStampUtc = DateTime.UtcNow;
        }

        public LoggingEvent(Type callerStackBoundaryDeclaringType, ILoggerRepository repository, LoggingEventData data, FixFlags fixedData)
        {
            m_callerStackBoundaryDeclaringType = callerStackBoundaryDeclaringType;
            m_repository = repository;

            m_data = data;
            m_fixFlags = fixedData;
        }

        public LoggingEvent(Type callerStackBoundaryDeclaringType, ILoggerRepository repository, LoggingEventData data) : this(callerStackBoundaryDeclaringType, repository, data, FixFlags.All)
        {
        }

        public LoggingEvent(LoggingEventData data) : this(null, null, data)
        {
        }

        /// <summary>
        /// 序列化构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected LoggingEvent(SerializationInfo info, StreamingContext context)
        {
            m_data.LoggerName = info.GetString("LoggerName");

            // Note we are deserializing the whole level object. That is the
            // name and the value. This value is correct for the source 
            // hierarchy but may not be for the target hierarchy that this
            // event may be re-logged into. If it is to be re-logged it may
            // be necessary to re-lookup the level based only on the name.
            m_data.Level = (Level)info.GetValue("Level", typeof(Level));

            m_data.Message = info.GetString("Message");
            m_data.ThreadName = info.GetString("ThreadName");
            m_data.TimeStampUtc = info.GetDateTime("TimeStamp").ToUniversalTime();
            m_data.LocationInfo = (LocationInfo)info.GetValue("LocationInfo", typeof(LocationInfo));
            m_data.UserName = info.GetString("UserName");
            m_data.ExceptionString = info.GetString("ExceptionString");
            m_data.Properties = (PropertiesDictionary)info.GetValue("Properties", typeof(PropertiesDictionary));
            m_data.Domain = info.GetString("Domain");
            m_data.Identity = info.GetString("Identity");

            // We have restored all the values of this instance, i.e. all the values are fixed
            // Set the fix flags otherwise the data values may be overwritten from the current environment.
            m_fixFlags = FixFlags.All;
        }

        #endregion

        #region Public Instance Properties

        public static DateTime StartTime
        {
            get { return SystemInfo.ProcessStartTimeUtc.ToLocalTime(); }
        }

        public static DateTime StartTimeUtc
        {
            get { return SystemInfo.ProcessStartTimeUtc; }
        }

        public Level Level
        {
            get { return m_data.Level; }
        }

        public DateTime TimeStamp
        {
            get { return m_data.TimeStampUtc.ToLocalTime(); }
        }

        public DateTime TimeStampUtc
        {
            get { return m_data.TimeStampUtc; }
        }

        public string LoggerName
        {
            get { return m_data.LoggerName; }
        }

        public LocationInfo LocationInformation
        {
            get
            {
                if (m_data.LocationInfo == null && this.m_cacheUpdatable)
                {
                    m_data.LocationInfo = new LocationInfo(m_callerStackBoundaryDeclaringType);
                }
                return m_data.LocationInfo;
            }
        }

        public object MessageObject
        {
            get { return m_message; }
        }

        public Exception ExceptionObject
        {
            get { return m_thrownException; }
        }

        public ILoggerRepository Repository
        {
            get { return m_repository; }
        }

        internal void EnsureRepository(ILoggerRepository repository)
        {
            if (repository != null)
            {
                m_repository = repository;
            }
        }

        public string RenderedMessage
        {
            get
            {
                if (m_data.Message == null && this.m_cacheUpdatable)
                {
                    if (m_message == null)
                    {
                        m_data.Message = "";
                    }
                    else if (m_message is string)
                    {
                        m_data.Message = (m_message as string);
                    }
                    else if (m_repository != null)
                    {
                        m_data.Message = m_repository.RendererMap.FindAndRender(m_message);
                    }
                    else
                    {
                        // Very last resort
                        m_data.Message = m_message.ToString();
                    }
                }
                return m_data.Message;
            }
        }

        public void WriteRenderedMessage(TextWriter writer)
        {
            if (m_data.Message != null)
            {
                writer.Write(m_data.Message);
            }
            else
            {
                if (m_message != null)
                {
                    if (m_message is string)
                    {
                        writer.Write(m_message as string);
                    }
                    else if (m_repository != null)
                    {
                        m_repository.RendererMap.FindAndRender(m_message, writer);
                    }
                    else
                    {
                        // Very last resort
                        writer.Write(m_message.ToString());
                    }
                }
            }
        }

        public string ThreadName
        {
            get
            {
                if (m_data.ThreadName == null && this.m_cacheUpdatable)
                {
                    m_data.ThreadName = System.Threading.Thread.CurrentThread.Name;
                    if (m_data.ThreadName == null || m_data.ThreadName.Length == 0)
                    {
                        // The thread name is not available. Therefore we
                        // go the the AppDomain to get the ID of the 
                        // current thread. (Why don't Threads know their own ID?)
                        try
                        {
                            m_data.ThreadName = SystemInfo.CurrentThreadId.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                        }
                        catch (System.Security.SecurityException)
                        {
                            // This security exception will occur if the caller does not have 
                            // some undefined set of SecurityPermission flags.
                            LogLog.Debug(declaringType, "Security exception while trying to get current thread ID. Error Ignored. Empty thread name.");

                            // As a last resort use the hash code of the Thread object
                            m_data.ThreadName = System.Threading.Thread.CurrentThread.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
                return m_data.ThreadName;
            }
        }

        public string UserName
        {
            get
            {
                if (m_data.UserName == null && this.m_cacheUpdatable)
                {
                    try
                    {
                        WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                        if (windowsIdentity != null && windowsIdentity.Name != null)
                        {
                            m_data.UserName = windowsIdentity.Name;
                        }
                        else
                        {
                            m_data.UserName = "";
                        }
                    }
                    catch (System.Security.SecurityException)
                    {
                        // This security exception will occur if the caller does not have 
                        // some undefined set of SecurityPermission flags.
                        LogLog.Debug(declaringType, "Security exception while trying to get current windows identity. Error Ignored. Empty user name.");

                        m_data.UserName = "";
                    }
                }
                return m_data.UserName;
            }
        }

        public string Identity
        {
            get
            {
                if (m_data.Identity == null && this.m_cacheUpdatable)
                {
                    try
                    {
                        if (System.Threading.Thread.CurrentPrincipal != null &&
                            System.Threading.Thread.CurrentPrincipal.Identity != null &&
                            System.Threading.Thread.CurrentPrincipal.Identity.Name != null)
                        {
                            m_data.Identity = System.Threading.Thread.CurrentPrincipal.Identity.Name;
                        }
                        else
                        {
                            m_data.Identity = "";
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // This exception will occur if System.Threading.Thread.CurrentPrincipal.Identity is not null but
                        // the getter of the property Name tries to access disposed objects.
                        // Seen to happen on IIS 7 or greater with windows authentication.
                        LogLog.Debug(declaringType, "Object disposed exception while trying to get current thread principal. Error Ignored. Empty identity name.");

                        m_data.Identity = "";
                    }
                    catch (System.Security.SecurityException)
                    {
                        // This security exception will occur if the caller does not have 
                        // some undefined set of SecurityPermission flags.
                        LogLog.Debug(declaringType, "Security exception while trying to get current thread principal. Error Ignored. Empty identity name.");

                        m_data.Identity = "";
                    }
                }
                return m_data.Identity;
            }
        }

        public string Domain
        {
            get
            {
                if (m_data.Domain == null && this.m_cacheUpdatable)
                {
                    m_data.Domain = SystemInfo.ApplicationFriendlyName;
                }
                return m_data.Domain;
            }
        }

        public PropertiesDictionary Properties
        {
            get
            {
                // If we have cached properties then return that otherwise changes will be lost
                if (m_data.Properties != null)
                {
                    return m_data.Properties;
                }

                if (m_eventProperties == null)
                {
                    m_eventProperties = new PropertiesDictionary();
                }
                return m_eventProperties;
            }
        }

        public FixFlags Fix
        {
            get { return m_fixFlags; }
            set { this.FixVolatileData(value); }
        }

        #endregion

        #region Public Instance Methods

        public LoggingEventData GetLoggingEventData()
        {
            return GetLoggingEventData(FixFlags.Partial);
        }

        public LoggingEventData GetLoggingEventData(FixFlags fixFlags)
        {
            Fix = fixFlags;
            return m_data;
        }

        public string GetExceptionString()
        {
            if (m_data.ExceptionString == null && this.m_cacheUpdatable)
            {
                if (m_thrownException != null)
                {
                    if (m_repository != null)
                    {
                        // Render exception using the repositories renderer map
                        m_data.ExceptionString = m_repository.RendererMap.FindAndRender(m_thrownException);
                    }
                    else
                    {
                        // Very last resort
                        m_data.ExceptionString = m_thrownException.ToString();
                    }
                }
                else
                {
                    m_data.ExceptionString = "";
                }
            }
            return m_data.ExceptionString;
        }

        public object LookupProperty(string key)
        {
            if (m_data.Properties != null)
            {
                return m_data.Properties[key];
            }
            if (m_compositeProperties == null)
            {
                CreateCompositeProperties();
            }
            return m_compositeProperties[key];
        }

        public PropertiesDictionary GetProperties()
        {
            if (m_data.Properties != null)
            {
                return m_data.Properties;
            }
            if (m_compositeProperties == null)
            {
                CreateCompositeProperties();
            }
            return m_compositeProperties.Flatten();
        }

        protected void FixVolatileData(FixFlags flags)
        {
            object forceCreation = null;

            //Unlock the cache so that new values can be stored
            //This may not be ideal if we are no longer in the correct context
            //and someone calls fix. 
            m_cacheUpdatable = true;

            // determine the flags that we are actually fixing
            FixFlags updateFlags = (FixFlags)((flags ^ m_fixFlags) & flags);

            if (updateFlags > 0)
            {
                if ((updateFlags & FixFlags.Message) != 0)
                {
                    // Force the message to be rendered
                    forceCreation = this.RenderedMessage;

                    m_fixFlags |= FixFlags.Message;
                }
                if ((updateFlags & FixFlags.ThreadName) != 0)
                {
                    // Grab the thread name
                    forceCreation = this.ThreadName;

                    m_fixFlags |= FixFlags.ThreadName;
                }

                if ((updateFlags & FixFlags.LocationInfo) != 0)
                {
                    // Force the location information to be loaded
                    forceCreation = this.LocationInformation;

                    m_fixFlags |= FixFlags.LocationInfo;
                }
                if ((updateFlags & FixFlags.UserName) != 0)
                {
                    // Grab the user name
                    forceCreation = this.UserName;

                    m_fixFlags |= FixFlags.UserName;
                }
                if ((updateFlags & FixFlags.Domain) != 0)
                {
                    // Grab the domain name
                    forceCreation = this.Domain;

                    m_fixFlags |= FixFlags.Domain;
                }
                if ((updateFlags & FixFlags.Identity) != 0)
                {
                    // Grab the identity
                    forceCreation = this.Identity;

                    m_fixFlags |= FixFlags.Identity;
                }

                if ((updateFlags & FixFlags.Exception) != 0)
                {
                    // Force the exception text to be loaded
                    forceCreation = GetExceptionString();

                    m_fixFlags |= FixFlags.Exception;
                }

                if ((updateFlags & FixFlags.Properties) != 0)
                {
                    CacheProperties();

                    m_fixFlags |= FixFlags.Properties;
                }
            }

            // avoid warning CS0219
            if (forceCreation != null)
            {
            }

            //Finaly lock everything we've cached.
            m_cacheUpdatable = false;
        }

        #endregion

        #region private Instance Methods

        private void CreateCompositeProperties()
        {
            CompositeProperties compositeProperties = new CompositeProperties();

            if (m_eventProperties != null)
            {
                compositeProperties.Add(m_eventProperties);
            }
#if !NETCF
            PropertiesDictionary logicalThreadProperties = LogicalThreadContext.Properties.GetProperties(false);
            if (logicalThreadProperties != null)
            {
                compositeProperties.Add(logicalThreadProperties);
            }
#endif
            PropertiesDictionary threadProperties = ThreadContext.Properties.GetProperties(false);
            if (threadProperties != null)
            {
                compositeProperties.Add(threadProperties);
            }

            // TODO: Add Repository Properties

            // event properties
            PropertiesDictionary eventProperties = new PropertiesDictionary();
            eventProperties[UserNameProperty] = UserName;
            eventProperties[IdentityProperty] = Identity;
            compositeProperties.Add(eventProperties);

            compositeProperties.Add(GlobalContext.Properties.GetReadOnlyProperties());
            m_compositeProperties = compositeProperties;
        }

        private void CacheProperties()
        {
            if (m_data.Properties == null && this.m_cacheUpdatable)
            {
                if (m_compositeProperties == null)
                {
                    CreateCompositeProperties();
                }

                PropertiesDictionary flattenedProperties = m_compositeProperties.Flatten();

                PropertiesDictionary fixedProperties = new PropertiesDictionary();

                // Validate properties
                foreach (DictionaryEntry entry in flattenedProperties)
                {
                    string key = entry.Key as string;

                    if (key != null)
                    {
                        object val = entry.Value;

                        // Fix any IFixingRequired objects
                        IFixingRequired fixingRequired = val as IFixingRequired;
                        if (fixingRequired != null)
                        {
                            val = fixingRequired.GetFixedObject();
                        }

                        // Strip keys with null values
                        if (val != null)
                        {
                            fixedProperties[key] = val;
                        }
                    }
                }

                m_data.Properties = fixedProperties;
            }
        }

        #endregion

        #region Implementation of ISerializable

        [System.Security.SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // The caller must call FixVolatileData before this object
            // can be serialized.

            info.AddValue("LoggerName", m_data.LoggerName);
            info.AddValue("Level", m_data.Level);
            info.AddValue("Message", m_data.Message);
            info.AddValue("ThreadName", m_data.ThreadName);
            // TODO: consider serializing UTC rather than local time.  Not implemented here because it
            // would give an unexpected result if client and server have different versions of this class.
            // info.AddValue("TimeStamp", m_data.TimeStampUtc);
#pragma warning disable 618
            info.AddValue("TimeStamp", m_data.TimeStamp);
#pragma warning restore 618
            info.AddValue("LocationInfo", m_data.LocationInfo);
            info.AddValue("UserName", m_data.UserName);
            info.AddValue("ExceptionString", m_data.ExceptionString);
            info.AddValue("Properties", m_data.Properties);
            info.AddValue("Domain", m_data.Domain);
            info.AddValue("Identity", m_data.Identity);
        }

        #endregion

        private LoggingEventData m_data;
        private CompositeProperties m_compositeProperties;
        private PropertiesDictionary m_eventProperties;

        /// <summary>
        /// caller 的完全限定类型(用于查找堆栈信息)
        /// </summary>
        private readonly Type m_callerStackBoundaryDeclaringType;

        private readonly object m_message;
        private readonly Exception m_thrownException;

        /// <summary>
        /// 
        /// </summary>
        private ILoggerRepository m_repository = null;
        /// <summary>
        /// 指示那些字段需要被输出
        /// </summary>
        private FixFlags m_fixFlags = FixFlags.None;
        /// <summary>
        /// 是否允许 <seealso cref="FixFlags"/> 更改
        /// </summary>
        private bool m_cacheUpdatable = true;

        #region Constants

        /// <summary>
        /// 当前主机名
        /// </summary>
        public const string HostNameProperty = "log4net:HostName";
        /// <summary>
        /// 当前线程地址
        /// </summary>
        public const string IdentityProperty = "log4net:Identity";
        /// <summary>
        /// 当前用户名
        /// </summary>
        public const string UserNameProperty = "log4net:UserName";

        #endregion

        private readonly static Type declaringType = typeof(LoggingEvent);
    }
}
