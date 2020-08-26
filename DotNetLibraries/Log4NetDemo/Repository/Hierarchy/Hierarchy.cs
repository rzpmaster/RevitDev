using Log4NetDemo.Appender;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class Hierarchy //: LoggerRepositorySkeleton, IBasicRepositoryConfigurator, IXmlRepositoryConfigurator
    {
        //public Hierarchy() : this(new DefaultLoggerFactory())
        //{
        //}

        //public Hierarchy(PropertiesDictionary properties) : this(properties, new DefaultLoggerFactory())
        //{
        //}

        //public Hierarchy(ILoggerFactory loggerFactory) : this(new PropertiesDictionary(), loggerFactory)
        //{
        //}

        //public Hierarchy(PropertiesDictionary properties, ILoggerFactory loggerFactory) : base(properties)
        //{
        //    if (loggerFactory == null)
        //    {
        //        throw new ArgumentNullException("loggerFactory");
        //    }

        //    m_defaultFactory = loggerFactory;

        //    m_ht = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
        //}


        //public event LoggerCreationEventHandler LoggerCreatedEvent
        //{
        //    add { m_loggerCreatedEvent += value; }
        //    remove { m_loggerCreatedEvent -= value; }
        //}

        //#region Public Instance Properties

        //public bool EmittedNoAppenderWarning
        //{
        //    get { return m_emittedNoAppenderWarning; }
        //    set { m_emittedNoAppenderWarning = value; }
        //}

        //public Logger Root
        //{
        //    get
        //    {
        //        if (m_root == null)
        //        {
        //            lock (this)
        //            {
        //                if (m_root == null)
        //                {
        //                    // Create the root logger
        //                    Logger root = m_defaultFactory.CreateLogger(this, null);
        //                    root.Hierarchy = this;

        //                    // Store root
        //                    m_root = root;
        //                }
        //            }
        //        }
        //        return m_root;
        //    }
        //}

        //public ILoggerFactory LoggerFactory
        //{
        //    get { return m_defaultFactory; }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            throw new ArgumentNullException("value");
        //        }
        //        m_defaultFactory = value;
        //    }
        //}

        //#endregion

        //#region Override Implementation of LoggerRepositorySkeleton

        //override public ILogger Exists(string name)
        //{
        //    if (name == null)
        //    {
        //        throw new ArgumentNullException("name");
        //    }

        //    lock (m_ht)
        //    {
        //        return m_ht[new LoggerKey(name)] as Logger;
        //    }
        //}

        //override public ILogger[] GetCurrentLoggers()
        //{
        //    // The accumulation in loggers is necessary because not all elements in
        //    // ht are Logger objects as there might be some ProvisionNodes
        //    // as well.
        //    lock (m_ht)
        //    {
        //        ArrayList loggers = new ArrayList(m_ht.Count);

        //        // Iterate through m_ht values
        //        foreach (object node in m_ht.Values)
        //        {
        //            if (node is Logger)
        //            {
        //                loggers.Add(node);
        //            }
        //        }
        //        return (Logger[])loggers.ToArray(typeof(Logger));
        //    }
        //}

        //override public ILogger GetLogger(string name)
        //{
        //    if (name == null)
        //    {
        //        throw new ArgumentNullException("name");
        //    }

        //    return GetLogger(name, m_defaultFactory);
        //}

        //override public void Shutdown()
        //{
        //    LogLog.Debug(declaringType, "Shutdown called on Hierarchy [" + this.Name + "]");

        //    // begin by closing nested appenders
        //    Root.CloseNestedAppenders();

        //    lock (m_ht)
        //    {
        //        ILogger[] currentLoggers = this.GetCurrentLoggers();

        //        foreach (Logger logger in currentLoggers)
        //        {
        //            logger.CloseNestedAppenders();
        //        }

        //        // then, remove all appenders
        //        Root.RemoveAllAppenders();

        //        foreach (Logger logger in currentLoggers)
        //        {
        //            logger.RemoveAllAppenders();
        //        }
        //    }

        //    base.Shutdown();
        //}

        //override public void ResetConfiguration()
        //{
        //    Root.Level = LevelMap.LookupWithDefault(Level.Debug);
        //    Threshold = LevelMap.LookupWithDefault(Level.All);

        //    // the synchronization is needed to prevent hashtable surprises
        //    lock (m_ht)
        //    {
        //        Shutdown(); // nested locks are OK	

        //        foreach (Logger l in this.GetCurrentLoggers())
        //        {
        //            l.Level = null;
        //            l.Additivity = true;
        //        }
        //    }

        //    base.ResetConfiguration();

        //    // Notify listeners
        //    OnConfigurationChanged(null);
        //}

        //override public void Log(LoggingEvent logEvent)
        //{
        //    if (logEvent == null)
        //    {
        //        throw new ArgumentNullException("logEvent");
        //    }

        //    this.GetLogger(logEvent.LoggerName, m_defaultFactory).Log(logEvent);
        //}

        //override public Appender.IAppender[] GetAppenders()
        //{
        //    ArrayList appenderList = new ArrayList();

        //    CollectAppenders(appenderList, Root);

        //    foreach (Logger logger in GetCurrentLoggers())
        //    {
        //        CollectAppenders(appenderList, logger);
        //    }

        //    return (Appender.IAppender[])appenderList.ToArray(typeof(Appender.IAppender));
        //}

        //#endregion

        //#region IBasicRepositoryConfigurator

        //void IBasicRepositoryConfigurator.Configure(IAppender appender)
        //{
        //    BasicRepositoryConfigure(appender);
        //}

        //void IBasicRepositoryConfigurator.Configure(params IAppender[] appenders)
        //{
        //    BasicRepositoryConfigure(appenders);
        //}

        //protected void BasicRepositoryConfigure(params IAppender[] appenders)
        //{
        //    ArrayList configurationMessages = new ArrayList();

        //    using (new LogLog.LogReceivedAdapter(configurationMessages))
        //    {
        //        foreach (IAppender appender in appenders)
        //        {
        //            Root.AddAppender(appender);
        //        }
        //    }

        //    Configured = true;

        //    ConfigurationMessages = configurationMessages;

        //    // Notify listeners
        //    OnConfigurationChanged(new ConfigurationChangedEventArgs(configurationMessages));
        //}

        //#endregion

        //#region IXmlRepositoryConfigurator

        //void IXmlRepositoryConfigurator.Configure(System.Xml.XmlElement element)
        //{
        //    XmlRepositoryConfigure(element);
        //}

        //protected void XmlRepositoryConfigure(System.Xml.XmlElement element)
        //{
        //    ArrayList configurationMessages = new ArrayList();

        //    using (new LogLog.LogReceivedAdapter(configurationMessages))
        //    {
        //        XmlHierarchyConfigurator config = new XmlHierarchyConfigurator(this);
        //        config.Configure(element);
        //    }

        //    Configured = true;

        //    ConfigurationMessages = configurationMessages;

        //    // Notify listeners
        //    OnConfigurationChanged(new ConfigurationChangedEventArgs(configurationMessages));
        //}

        //#endregion

        //protected virtual void OnLoggerCreationEvent(Logger logger)
        //{
        //    m_loggerCreatedEvent?.Invoke(this, new LoggerCreationEventArgs(logger));
        //}

        private ILoggerFactory m_defaultFactory;

        private System.Collections.Hashtable m_ht;
        private Logger m_root;

        private bool m_emittedNoAppenderWarning = false;

        private event LoggerCreationEventHandler m_loggerCreatedEvent;

        private readonly static Type declaringType = typeof(Hierarchy);
    }
}
