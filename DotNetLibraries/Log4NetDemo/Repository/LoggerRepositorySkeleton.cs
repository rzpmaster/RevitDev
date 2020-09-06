using System;
using System.Collections;
using System.Threading;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.ObjectRenderer;
using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;

namespace Log4NetDemo.Repository
{
    public abstract class LoggerRepositorySkeleton : ILoggerRepository, IFlushable
    {
        protected LoggerRepositorySkeleton() : this(new PropertiesDictionary())
        {
        }

        protected LoggerRepositorySkeleton(PropertiesDictionary properties)
        {
            m_properties = properties;
            m_rendererMap = new RendererMap();
            //m_pluginMap = new PluginMap(this);
            m_levelMap = new LevelMap();
            m_configurationMessages = EmptyCollection.Instance;
            m_configured = false;

            AddBuiltinLevels();

            // Don't disable any levels by default.
            m_threshold = Level.All;
        }

        #region Implementation of ILoggerRepository

        virtual public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        virtual public Level Threshold
        {
            get { return m_threshold; }
            set
            {
                if (value != null)
                {
                    m_threshold = value;
                }
                else
                {
                    // Must not set threshold to null
                    LogLog.Warn(declaringType, "LoggerRepositorySkeleton: Threshold cannot be set to null. Setting to ALL");
                    m_threshold = Level.All;
                }
            }
        }

        virtual public RendererMap RendererMap
        {
            get { return m_rendererMap; }
        }

        //virtual public PluginMap PluginMap
        //{
        //    get { return m_pluginMap; }
        //}

        virtual public LevelMap LevelMap
        {
            get { return m_levelMap; }
        }

        abstract public ILogger Exists(string name);

        abstract public ILogger[] GetCurrentLoggers();

        abstract public ILogger GetLogger(string name);

        /// <summary>
        /// Shutdown the repository
        /// </summary>
        /// <remarks>
        /// <para>
        /// Shutdown the repository. Can be overridden in a subclass.
        /// This base class implementation notifies the <see cref="ShutdownEvent"/>
        /// listeners and all attached plugins of the shutdown event.
        /// </para>
        /// </remarks>
        virtual public void Shutdown()
        {
            // Shutdown attached plugins
            //foreach (IPlugin plugin in PluginMap.AllPlugins)
            //{
            //    plugin.Shutdown();
            //}

            // Notify listeners
            OnShutdown(null);
        }

        /// <summary>
        /// Reset the repositories configuration to a default state
        /// </summary>
        /// <remarks>
        /// <para>
        /// 将此 ILoggerRepository 中的所有实例初始化为默认状态。
        /// </para>
        /// <para>
        /// 谨慎使用，此方法会阻止所有日志记录，知道重置操作完成
        /// </para>
        /// </remarks>
        virtual public void ResetConfiguration()
        {
            // Clear internal data structures
            m_rendererMap.Clear();
            m_levelMap.Clear();
            m_configurationMessages = EmptyCollection.Instance;

            // Add the predefined levels to the map
            AddBuiltinLevels();

            Configured = false;

            // Notify listeners
            OnConfigurationReset(null);
        }

        abstract public void Log(LoggingEvent logEvent);

        /// <summary>
        /// 指示是否配置完成
        /// </summary>
        virtual public bool Configured
        {
            get { return m_configured; }
            set { m_configured = value; }
        }

        /// <summary>
        /// 配置期间的日志消息列表
        /// </summary>
        virtual public ICollection ConfigurationMessages
        {
            get { return m_configurationMessages; }
            set { m_configurationMessages = value; }
        }

        public event LoggerRepositoryShutdownEventHandler ShutdownEvent
        {
            add { m_shutdownEvent += value; }
            remove { m_shutdownEvent -= value; }
        }

        public event LoggerRepositoryConfigurationResetEventHandler ConfigurationReset
        {
            add { m_configurationResetEvent += value; }
            remove { m_configurationResetEvent -= value; }
        }

        public event LoggerRepositoryConfigurationChangedEventHandler ConfigurationChanged
        {
            add { m_configurationChangedEvent += value; }
            remove { m_configurationChangedEvent -= value; }
        }

        public PropertiesDictionary Properties
        {
            get { return m_properties; }
        }

        abstract public Appender.IAppender[] GetAppenders();

        #endregion

        #region Implementation of IFlushable

        public bool Flush(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1) throw new ArgumentOutOfRangeException("millisecondsTimeout", "Timeout must be -1 (Timeout.Infinite) or non-negative");

            // Assume success until one of the appenders fails
            bool result = true;

            // Use DateTime.UtcNow rather than a System.Diagnostics.Stopwatch for compatibility with .NET 1.x
            DateTime startTimeUtc = DateTime.UtcNow;

            // Do buffering appenders first.  These may be forwarding to other appenders
            foreach (Appender.IAppender appender in GetAppenders())
            {
                IFlushable flushable = appender as IFlushable;
                if (flushable == null) continue;
                if (appender is Appender.BufferingAppenderSkeleton)
                {
                    int timeout = GetWaitTime(startTimeUtc, millisecondsTimeout);
                    if (!flushable.Flush(timeout)) result = false;
                }
            }

            // Do non-buffering appenders.
            foreach (Appender.IAppender appender in GetAppenders())
            {
                IFlushable flushable = appender as IFlushable;
                if (flushable == null) continue;
                if (!(appender is Appender.BufferingAppenderSkeleton))
                {
                    int timeout = GetWaitTime(startTimeUtc, millisecondsTimeout);
                    if (!flushable.Flush(timeout)) result = false;
                }
            }

            return result;
        }

        #endregion

        private void AddBuiltinLevels()
        {
            // Add the predefined levels to the map
            m_levelMap.Add(Level.Off);

            // Unrecoverable errors
            m_levelMap.Add(Level.Emergency);
            m_levelMap.Add(Level.Fatal);
            m_levelMap.Add(Level.Alert);

            // Recoverable errors
            m_levelMap.Add(Level.Critical);
            m_levelMap.Add(Level.Severe);
            m_levelMap.Add(Level.Error);
            m_levelMap.Add(Level.Warn);

            // Information
            m_levelMap.Add(Level.Notice);
            m_levelMap.Add(Level.Info);

            // Debug
            m_levelMap.Add(Level.Debug);
            m_levelMap.Add(Level.Fine);
            m_levelMap.Add(Level.Trace);
            m_levelMap.Add(Level.Finer);
            m_levelMap.Add(Level.Verbose);
            m_levelMap.Add(Level.Finest);

            m_levelMap.Add(Level.All);
        }

        virtual public void AddRenderer(Type typeToRender, IObjectRenderer rendererInstance)
        {
            if (typeToRender == null)
            {
                throw new ArgumentNullException("typeToRender");
            }
            if (rendererInstance == null)
            {
                throw new ArgumentNullException("rendererInstance");
            }

            m_rendererMap.Put(typeToRender, rendererInstance);
        }

        protected virtual void OnShutdown(EventArgs e)
        {
            if (e == null)
            {
                e = EventArgs.Empty;
            }

            m_shutdownEvent?.Invoke(this, e);
        }

        protected virtual void OnConfigurationReset(EventArgs e)
        {
            if (e == null)
            {
                e = EventArgs.Empty;
            }

            m_configurationResetEvent?.Invoke(this, e);
        }

        protected virtual void OnConfigurationChanged(EventArgs e)
        {
            if (e == null)
            {
                e = EventArgs.Empty;
            }

            m_configurationChangedEvent?.Invoke(this, e);
        }

        public void RaiseConfigurationChanged(EventArgs e)
        {
            OnConfigurationChanged(e);
        }

        private static int GetWaitTime(DateTime startTimeUtc, int millisecondsTimeout)
        {
            if (millisecondsTimeout == Timeout.Infinite) return Timeout.Infinite;
            if (millisecondsTimeout == 0) return 0;

            int elapsedMilliseconds = (int)(DateTime.UtcNow - startTimeUtc).TotalMilliseconds;
            int timeout = millisecondsTimeout - elapsedMilliseconds;
            if (timeout < 0) timeout = 0;
            return timeout;
        }

        private string m_name;
        private RendererMap m_rendererMap;
        //private PluginMap m_pluginMap;
        private LevelMap m_levelMap;
        private Level m_threshold;

        private bool m_configured;
        private ICollection m_configurationMessages;

        private event LoggerRepositoryShutdownEventHandler m_shutdownEvent;
        private event LoggerRepositoryConfigurationResetEventHandler m_configurationResetEvent;
        private event LoggerRepositoryConfigurationChangedEventHandler m_configurationChangedEvent;
        private PropertiesDictionary m_properties;

        private readonly static Type declaringType = typeof(LoggerRepositorySkeleton);
    }
}
