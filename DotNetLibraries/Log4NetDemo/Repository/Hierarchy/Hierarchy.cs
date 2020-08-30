using Log4NetDemo.Appender;
using Log4NetDemo.Appender.AppenderAttach;
using Log4NetDemo.Configration;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;
using System;
using System.Collections;

namespace Log4NetDemo.Repository.Hierarchy
{
    public delegate void LoggerCreationEventHandler(object sender, LoggerCreationEventArgs e);

    public class LoggerCreationEventArgs : EventArgs
    {
        private Logger m_log;

        public LoggerCreationEventArgs(Logger log)
        {
            m_log = log;
        }

        public Logger Logger
        {
            get { return m_log; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Hierarchy : LoggerRepositorySkeleton, IBasicRepositoryConfigurator, IXmlRepositoryConfigurator
    {
        #region Constructors

        public Hierarchy() : this(new DefaultLoggerFactory())
        {
        }

        public Hierarchy(PropertiesDictionary properties) : this(properties, new DefaultLoggerFactory())
        {
        }

        public Hierarchy(ILoggerFactory loggerFactory) : this(new PropertiesDictionary(), loggerFactory)
        {
        }

        public Hierarchy(PropertiesDictionary properties, ILoggerFactory loggerFactory) : base(properties)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException("loggerFactory");
            }

            m_defaultFactory = loggerFactory;

            m_ht = Hashtable.Synchronized(new Hashtable());
        }

        #endregion

        public event LoggerCreationEventHandler LoggerCreatedEvent
        {
            add { m_loggerCreatedEvent += value; }
            remove { m_loggerCreatedEvent -= value; }
        }

        protected virtual void OnLoggerCreationEvent(Logger logger)
        {
            m_loggerCreatedEvent?.Invoke(this, new LoggerCreationEventArgs(logger));
        }

        #region Public Instance Properties

        /// <summary>
        /// 是否已经警告过了（当树形 Logger 中没有一个有 Appender 时，只需要警告一次）
        /// </summary>
        public bool EmittedNoAppenderWarning
        {
            get { return m_emittedNoAppenderWarning; }
            set { m_emittedNoAppenderWarning = value; }
        }

        public Logger Root
        {
            get
            {
                if (m_root == null)
                {
                    lock (this)
                    {
                        if (m_root == null)
                        {
                            // Create the root logger
                            Logger root = m_defaultFactory.CreateLogger(this, null);
                            root.Hierarchy = this;

                            // Store root
                            m_root = root;
                        }
                    }
                }
                return m_root;
            }
        }

        public ILoggerFactory LoggerFactory
        {
            get { return m_defaultFactory; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                m_defaultFactory = value;
            }
        }

        #endregion

        #region Override Implementation of LoggerRepositorySkeleton

        override public ILogger Exists(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            lock (m_ht)
            {
                return m_ht[new LoggerKey(name)] as Logger;
            }
        }

        override public ILogger[] GetCurrentLoggers()
        {
            // The accumulation in loggers is necessary because not all elements in
            // ht are Logger objects as there might be some ProvisionNodes
            // as well.
            lock (m_ht)
            {
                ArrayList loggers = new ArrayList(m_ht.Count);

                // Iterate through m_ht values
                foreach (object node in m_ht.Values)
                {
                    if (node is Logger)
                    {
                        loggers.Add(node);
                    }
                }
                return (Logger[])loggers.ToArray(typeof(Logger));
            }
        }

        override public ILogger GetLogger(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return GetLogger(name, m_defaultFactory);
        }

        override public void Shutdown()
        {
            LogLog.Debug(declaringType, "Shutdown called on Hierarchy [" + this.Name + "]");

            // begin by closing nested appenders
            Root.CloseNestedAppenders();

            lock (m_ht)
            {
                ILogger[] currentLoggers = this.GetCurrentLoggers();

                foreach (Logger logger in currentLoggers)
                {
                    logger.CloseNestedAppenders();
                }

                // then, remove all appenders
                Root.RemoveAllAppenders();

                foreach (Logger logger in currentLoggers)
                {
                    logger.RemoveAllAppenders();
                }
            }

            base.Shutdown();
        }

        override public void ResetConfiguration()
        {
            Root.Level = LevelMap.LookupWithDefault(Level.Debug);
            Threshold = LevelMap.LookupWithDefault(Level.All);

            // the synchronization is needed to prevent hashtable surprises
            lock (m_ht)
            {
                Shutdown(); // nested locks are OK	

                foreach (Logger l in this.GetCurrentLoggers())
                {
                    l.Level = null;
                    l.Additivity = true;
                }
            }

            base.ResetConfiguration();

            // Notify listeners
            OnConfigurationChanged(null);
        }

        override public void Log(LoggingEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException("logEvent");
            }

            this.GetLogger(logEvent.LoggerName, m_defaultFactory).Log(logEvent);
        }

        override public Appender.IAppender[] GetAppenders()
        {
            ArrayList appenderList = new ArrayList();

            CollectAppenders(appenderList, Root);

            foreach (Logger logger in GetCurrentLoggers())
            {
                CollectAppenders(appenderList, logger);
            }

            return (Appender.IAppender[])appenderList.ToArray(typeof(Appender.IAppender));
        }

        #endregion

        #region Public Instance Methods

        public bool IsDisabled(Level level)
        {
            // Cast level to object for performance
            if ((object)level == null)
            {
                throw new ArgumentNullException("level");
            }

            if (Configured)
            {
                return Threshold > level;
            }
            else
            {
                // If not configured the hierarchy is effectively disabled
                return true;
            }
        }

        public void Clear()
        {
            lock (m_ht)
            {
                m_ht.Clear();
            }
        }

        public Logger GetLogger(string name, ILoggerFactory factory)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            LoggerKey key = new LoggerKey(name);

            // Synchronize to prevent write conflicts. Read conflicts (in
            // GetEffectiveLevel() method) are possible only if variable
            // assignments are non-atomic.

            lock (m_ht)
            {
                Logger logger = null;

                Object node = m_ht[key];
                if (node == null)
                {
                    logger = factory.CreateLogger(this, name);
                    logger.Hierarchy = this;
                    m_ht[key] = logger;
                    UpdateParents(logger);
                    OnLoggerCreationEvent(logger);
                    return logger;
                }

                Logger nodeLogger = node as Logger;
                if (nodeLogger != null)
                {
                    return nodeLogger;
                }

                ProvisionNode nodeProvisionNode = node as ProvisionNode;
                if (nodeProvisionNode != null)
                {
                    logger = factory.CreateLogger(this, name);
                    logger.Hierarchy = this;
                    m_ht[key] = logger;
                    UpdateChildren(nodeProvisionNode, logger);
                    UpdateParents(logger);
                    OnLoggerCreationEvent(logger);
                    return logger;
                }

                // It should be impossible to arrive here but let's keep the compiler happy.
                return null;
            }
        }

        #endregion

        #region Private Instance Methods

        private void UpdateParents(Logger log)
        {
            string name = log.Name;
            int length = name.Length;
            bool parentFound = false;

            // if name = "w.x.y.z", loop through "w.x.y", "w.x" and "w", but not "w.x.y.z" 
            for (int i = name.LastIndexOf('.', length - 1); i >= 0; i = name.LastIndexOf('.', i - 1))
            {
                string substr = name.Substring(0, i);

                LoggerKey key = new LoggerKey(substr); // simple constructor
                Object node = m_ht[key];
                // Create a provision node for a future parent.
                if (node == null)
                {
                    ProvisionNode pn = new ProvisionNode(log);
                    m_ht[key] = pn;
                }
                else
                {
                    Logger nodeLogger = node as Logger;
                    if (nodeLogger != null)
                    {
                        parentFound = true;
                        log.Parent = nodeLogger;
                        break; // no need to update the ancestors of the closest ancestor
                    }
                    else
                    {
                        ProvisionNode nodeProvisionNode = node as ProvisionNode;
                        if (nodeProvisionNode != null)
                        {
                            nodeProvisionNode.Add(log);
                        }
                        else
                        {
                            LogLog.Error(declaringType, "Unexpected object type [" + node.GetType() + "] in ht.", new LogException());
                        }
                    }
                }
                if (i == 0)
                {
                    // logger name starts with a dot
                    // and we've hit the start
                    break;
                }
            }

            // If we could not find any existing parents, then link with root.
            if (!parentFound)
            {
                log.Parent = this.Root;
            }
        }

        private static void UpdateChildren(ProvisionNode pn, Logger log)
        {
            for (int i = 0; i < pn.Count; i++)
            {
                Logger childLogger = (Logger)pn[i];

                // Unless this child already points to a correct (lower) parent,
                // make log.Parent point to childLogger.Parent and childLogger.Parent to log.
                if (!childLogger.Parent.Name.StartsWith(log.Name))
                {
                    log.Parent = childLogger.Parent;
                    childLogger.Parent = log;
                }
            }
        }

        internal void AddLevel(LevelEntry levelEntry)
        {
            if (levelEntry == null) throw new ArgumentNullException("levelEntry");
            if (levelEntry.Name == null) throw new ArgumentNullException("levelEntry.Name");

            // Lookup replacement value
            if (levelEntry.Value == -1)
            {
                Level previousLevel = LevelMap[levelEntry.Name];
                if (previousLevel == null)
                {
                    throw new InvalidOperationException("Cannot redefine level [" + levelEntry.Name + "] because it is not defined in the LevelMap. To define the level supply the level value.");
                }

                levelEntry.Value = previousLevel.Value;
            }

            LevelMap.Add(levelEntry.Name, levelEntry.Value, levelEntry.DisplayName);
        }

        internal void AddProperty(PropertyEntry propertyEntry)
        {
            if (propertyEntry == null) throw new ArgumentNullException("propertyEntry");
            if (propertyEntry.Key == null) throw new ArgumentNullException("propertyEntry.Key");

            Properties[propertyEntry.Key] = propertyEntry.Value;
        }

        #endregion

        #region Private Static Methods

        private static void CollectAppender(ArrayList appenderList, IAppender appender)
        {
            if (!appenderList.Contains(appender))
            {
                appenderList.Add(appender);

                IAppenderAttachable container = appender as IAppenderAttachable;
                if (container != null)
                {
                    CollectAppenders(appenderList, container);
                }
            }
        }

        private static void CollectAppenders(ArrayList appenderList, IAppenderAttachable container)
        {
            foreach (IAppender appender in container.Appenders)
            {
                CollectAppender(appenderList, appender);
            }
        }

        #endregion

        #region IBasicRepositoryConfigurator

        void IBasicRepositoryConfigurator.Configure(IAppender appender)
        {
            BasicRepositoryConfigure(appender);
        }

        void IBasicRepositoryConfigurator.Configure(params IAppender[] appenders)
        {
            BasicRepositoryConfigure(appenders);
        }

        protected void BasicRepositoryConfigure(params IAppender[] appenders)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                foreach (IAppender appender in appenders)
                {
                    Root.AddAppender(appender);
                }
            }

            Configured = true;

            ConfigurationMessages = configurationMessages;

            // Notify listeners
            OnConfigurationChanged(new ConfigurationChangedEventArgs(configurationMessages));
        }

        #endregion

        #region IXmlRepositoryConfigurator

        void IXmlRepositoryConfigurator.Configure(System.Xml.XmlElement element)
        {
            XmlRepositoryConfigure(element);
        }

        protected void XmlRepositoryConfigure(System.Xml.XmlElement element)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                XmlHierarchyConfigurator config = new XmlHierarchyConfigurator(this);
                config.Configure(element);
            }

            Configured = true;

            ConfigurationMessages = configurationMessages;

            // Notify listeners
            OnConfigurationChanged(new ConfigurationChangedEventArgs(configurationMessages));
        }

        #endregion

        internal sealed class ProvisionNode : ArrayList
        {
            internal ProvisionNode(Logger log) : base()
            {
                this.Add(log);
            }
        }

        /// <summary>
        /// 内部维护的哈希表
        /// key:LoggerKey value:ILogger
        /// </summary>
        private Hashtable m_ht;

        private Logger m_root;
        private ILoggerFactory m_defaultFactory;

        private bool m_emittedNoAppenderWarning = false;
        private event LoggerCreationEventHandler m_loggerCreatedEvent;

        private readonly static Type declaringType = typeof(Hierarchy);
    }
}
