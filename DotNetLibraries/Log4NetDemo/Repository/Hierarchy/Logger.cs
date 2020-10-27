using Log4NetDemo.Appender;
using Log4NetDemo.Appender.AppenderAttach;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Repository.Hierarchy
{
    /// <summary>
    /// 维护一个 Logger name 的哈希值
    /// </summary>
    internal sealed class LoggerKey
    {
        internal LoggerKey(string name)
        {
            m_name = string.Intern(name);
            m_hashCache = name.GetHashCode();
        }

        override public int GetHashCode()
        {
            return m_hashCache;
        }

        override public bool Equals(object obj)
        {
            // Compare reference type of this against argument
            if (((object)this) == obj)
            {
                return true;
            }

            LoggerKey objKey = obj as LoggerKey;
            if (objKey != null)
            {
                return (((object)m_name) == ((object)objKey.m_name));
            }
            return false;
        }

        private readonly string m_name;
        private readonly int m_hashCache;
    }

    /// <summary>
    /// Implementation of <see cref="ILogger"/> used by <see cref="Hierarchy"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// 内部使用请求 <see cref="ILogger"/> 实例，
    /// 如果外部 Applications 需要使用的话，要用<see cref="Core.LogManager"/>得到。
	/// Internal class used to provide implementation of <see cref="ILogger"/>
	/// interface. Applications should use <see cref="Core.LogManager"/> to get
	/// logger instances.
	/// </para>
    /// <para>
    /// 分层记录日志，<see cref="Hierarchy"/> 将 Logger 实例组织为 树型层次结构
    /// The <see cref="Hierarchy"/> organizes the <see cref="Logger"/>
    /// instances into a rooted tree hierarchy.
    /// </para>
    /// <para>抽象类，由<see cref="ILoggerFactory"/>创建实例，给<see cref="Hierarchy"/>使用。</para>
    /// </remarks>
    public abstract class Logger : IAppenderAttachable, ILogger
    {
        protected Logger(string name)
        {
            m_name = string.Intern(name);
        }

        #region Public Instance Properties

        virtual public Logger Parent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }

        /// <summary>
        /// 指示 子Logger 是否继承 父Logger 的 Appender
        /// </summary>
        virtual public bool Additivity
        {
            get { return m_additive; }
            set { m_additive = value; }
        }

        /// <summary>
        /// 获取树形结构 Logger 的有效等级，从下往上找
        /// </summary>
        virtual public Level EffectiveLevel
        {
            get
            {
                for (Logger c = this; c != null; c = c.m_parent)
                {
                    Level level = c.m_level;

                    // Casting level to Object for performance, otherwise the overloaded operator is called
                    if ((object)level != null)
                    {
                        return level;
                    }
                }
                return null; // If reached will cause an NullPointerException.
            }
        }

        virtual public Hierarchy Hierarchy
        {
            get { return m_hierarchy; }
            set { m_hierarchy = value; }
        }

        virtual public Level Level
        {
            get { return m_level; }
            set { m_level = value; }
        }

        #endregion

        #region Implementation of IAppenderAttachable

        /// 
        /// 对 m_appenderAttachedImpl 字段的维护
        /// 实际上就是调用 m_appenderAttachedImpl 的方法
        /// 

        virtual public void AddAppender(IAppender newAppender)
        {
            if (newAppender == null)
            {
                throw new ArgumentNullException("newAppender");
            }

            m_appenderLock.AcquireWriterLock();
            try
            {
                if (m_appenderAttachedImpl == null)
                {
                    m_appenderAttachedImpl = new AppenderAttachedImpl();
                }
                m_appenderAttachedImpl.AddAppender(newAppender);
            }
            finally
            {
                m_appenderLock.ReleaseWriterLock();
            }
        }

        virtual public AppenderCollection Appenders
        {
            get
            {
                m_appenderLock.AcquireReaderLock();
                try
                {
                    if (m_appenderAttachedImpl == null)
                    {
                        return AppenderCollection.EmptyCollection;
                    }
                    else
                    {
                        return m_appenderAttachedImpl.Appenders;
                    }
                }
                finally
                {
                    m_appenderLock.ReleaseReaderLock();
                }
            }
        }

        virtual public IAppender GetAppender(string name)
        {
            m_appenderLock.AcquireReaderLock();
            try
            {
                if (m_appenderAttachedImpl == null || name == null)
                {
                    return null;
                }

                return m_appenderAttachedImpl.GetAppender(name);
            }
            finally
            {
                m_appenderLock.ReleaseReaderLock();
            }
        }

        virtual public void RemoveAllAppenders()
        {
            m_appenderLock.AcquireWriterLock();
            try
            {
                if (m_appenderAttachedImpl != null)
                {
                    m_appenderAttachedImpl.RemoveAllAppenders();
                    m_appenderAttachedImpl = null;
                }
            }
            finally
            {
                m_appenderLock.ReleaseWriterLock();
            }
        }

        virtual public IAppender RemoveAppender(IAppender appender)
        {
            m_appenderLock.AcquireWriterLock();
            try
            {
                if (appender != null && m_appenderAttachedImpl != null)
                {
                    return m_appenderAttachedImpl.RemoveAppender(appender);
                }
            }
            finally
            {
                m_appenderLock.ReleaseWriterLock();
            }
            return null;
        }

        virtual public IAppender RemoveAppender(string name)
        {
            m_appenderLock.AcquireWriterLock();
            try
            {
                if (name != null && m_appenderAttachedImpl != null)
                {
                    return m_appenderAttachedImpl.RemoveAppender(name);
                }
            }
            finally
            {
                m_appenderLock.ReleaseWriterLock();
            }
            return null;
        }

        #endregion

        #region Implementation of ILogger

        virtual public string Name
        {
            get { return m_name; }
        }

        virtual public void Log(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception)
        {
            try
            {
                if (IsEnabledFor(level))
                {
                    ForcedLog((callerStackBoundaryDeclaringType != null) ? callerStackBoundaryDeclaringType : declaringType, level, message, exception);
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Exception while logging", ex);
            }
        }

        virtual public void Log(LoggingEvent logEvent)
        {
            try
            {
                if (logEvent != null)
                {
                    if (IsEnabledFor(logEvent.Level))
                    {
                        ForcedLog(logEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogLog.Error(declaringType, "Exception while logging", ex);
            }
        }

        virtual public bool IsEnabledFor(Level level)
        {
            try
            {
                if (level != null)
                {
                    if (m_hierarchy.IsDisabled(level))
                    {
                        return false;
                    }
                    return level >= this.EffectiveLevel;
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Exception while logging", ex);
            }
            return false;
        }

        public ILoggerRepository Repository
        {
            get { return m_hierarchy; }
        }

        #endregion

        virtual protected void CallAppenders(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            int writes = 0;

            for (Logger c = this; c != null; c = c.m_parent)
            {
                if (c.m_appenderAttachedImpl != null)
                {
                    // Protected against simultaneous call to addAppender, removeAppender,...
                    c.m_appenderLock.AcquireReaderLock();
                    try
                    {
                        if (c.m_appenderAttachedImpl != null)
                        {
                            writes += c.m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvent);
                        }
                    }
                    finally
                    {
                        c.m_appenderLock.ReleaseReaderLock();
                    }
                }

                if (!c.m_additive)
                {
                    break;
                }
            }

            // No appenders in hierarchy, warn user only once.
            //
            // Note that by including the AppDomain values for the currently running
            // thread, it becomes much easier to see which application the warning
            // is from, which is especially helpful in a multi-AppDomain environment
            // (like IIS with multiple VDIRS).  Without this, it can be difficult
            // or impossible to determine which .config file is missing appender
            // definitions.
            //

            // 层次结构中没有 Appender 只警告一次
            // 通过查看当前 AppDomain ，可以查看警告来自哪个 应用程序

            if (!m_hierarchy.EmittedNoAppenderWarning && writes == 0)
            {
                m_hierarchy.EmittedNoAppenderWarning = true;
                LogLog.Debug(declaringType, "No appenders could be found for logger [" + Name + "] repository [" + Repository.Name + "]");
                LogLog.Debug(declaringType, "Please initialize the log4net system properly.");

                try
                {
                    LogLog.Debug(declaringType, "    Current AppDomain context information: ");
                    LogLog.Debug(declaringType, "       BaseDirectory   : " + SystemInfo.ApplicationBaseDirectory);
                    LogLog.Debug(declaringType, "       FriendlyName    : " + AppDomain.CurrentDomain.FriendlyName);
                    LogLog.Debug(declaringType, "       DynamicDirectory: " + AppDomain.CurrentDomain.DynamicDirectory);
                }
                catch (System.Security.SecurityException)
                {
                    // Insufficient permissions to display info from the AppDomain
                    // 权限不足
                }
            }
        }

        /// <summary>
        /// 关闭所有实现<see cref=“IAppenderAttachable”/>接口的 Appender
        /// </summary>
        virtual public void CloseNestedAppenders()
        {
            m_appenderLock.AcquireWriterLock();
            try
            {
                if (m_appenderAttachedImpl != null)
                {
                    AppenderCollection appenders = m_appenderAttachedImpl.Appenders;
                    foreach (IAppender appender in appenders)
                    {
                        if (appender is IAppenderAttachable)
                        {
                            appender.Close();
                        }
                    }
                }
            }
            finally
            {
                m_appenderLock.ReleaseWriterLock();
            }
        }

        virtual public void Log(Level level, object message, Exception exception)
        {
            if (IsEnabledFor(level))
            {
                ForcedLog(declaringType, level, message, exception);
            }
        }

        virtual protected void ForcedLog(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception)
        {
            CallAppenders(new LoggingEvent(callerStackBoundaryDeclaringType, this.Hierarchy, this.Name, level, message, exception));
        }

        virtual protected void ForcedLog(LoggingEvent logEvent)
        {
            // The logging event may not have been created by this logger
            // the Repository may not be correctly set on the event. This
            // is required for the appenders to correctly lookup renderers etc...
            logEvent.EnsureRepository(this.Hierarchy);

            CallAppenders(logEvent);
        }

        private readonly string m_name;
        private Level m_level;
        private Logger m_parent;
        private Hierarchy m_hierarchy;
        private AppenderAttachedImpl m_appenderAttachedImpl;

        /// <summary>
        /// 是否继承 其父节点Logger 的 Appender
        /// </summary>
        private bool m_additive = true;

        private readonly ReaderWriterLock m_appenderLock = new ReaderWriterLock();

        private readonly static Type declaringType = typeof(Logger);
    }
}
