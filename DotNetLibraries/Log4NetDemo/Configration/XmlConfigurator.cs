using Log4NetDemo.Core;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace Log4NetDemo.Configration
{
    public sealed class XmlConfigurator
    {
        private XmlConfigurator()
        {
        }

        #region Configure static methods

        static public ICollection Configure()
        {
            return Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
        }

        static public ICollection Configure(ILoggerRepository repository)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(XmlElement element)
        {
            ArrayList configurationMessages = new ArrayList();

            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigureFromXml(repository, element);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(ILoggerRepository repository, XmlElement element)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                LogLog.Debug(declaringType, "configuring repository [" + repository.Name + "] using XML element");

                InternalConfigureFromXml(repository, element);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(FileInfo configFile)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(LogManager.GetRepository(Assembly.GetCallingAssembly()), configFile);
            }

            return configurationMessages;
        }

        static public ICollection Configure(ILoggerRepository repository, FileInfo configFile)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, configFile);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(Stream configStream)
        {
            ArrayList configurationMessages = new ArrayList();

            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, configStream);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(ILoggerRepository repository, Stream configStream)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, configStream);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(Uri configUri)
        {
            ArrayList configurationMessages = new ArrayList();

            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, configUri);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection Configure(ILoggerRepository repository, Uri configUri)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigure(repository, configUri);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        #endregion

        #region Private Configure Static Methods

        /// <summary>
        /// 读取配置文件中的节点，配置 ILoggerRepository
        /// </summary>
        /// <param name="repository"></param>
        static private void InternalConfigure(ILoggerRepository repository)
        {
            LogLog.Debug(declaringType, "configuring repository [" + repository.Name + "] using .config file section");

            try
            {
                LogLog.Debug(declaringType, "Application config file is [" + SystemInfo.ConfigurationFileLocation + "]");
            }
            catch
            {
                // ignore error
                LogLog.Debug(declaringType, "Application config file location unknown");
            }

            try
            {
                XmlElement configElement = System.Configuration.ConfigurationManager.GetSection("log4net") as XmlElement;


                if (configElement == null)
                {
                    // Failed to load the xml config using configuration settings handler
                    LogLog.Error(declaringType, "Failed to find configuration section 'log4net' in the application's .config file. Check your .config file for the <log4net> and <configSections> elements. The configuration section should look like: <section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler,log4net\" />");
                }
                else
                {
                    // Configure using the xml loaded from the config file
                    InternalConfigureFromXml(repository, configElement);
                }
            }
            catch (System.Configuration.ConfigurationException confEx)
            {
                if (confEx.BareMessage.IndexOf("Unrecognized element") >= 0)
                {
                    // Looks like the XML file is not valid
                    LogLog.Error(declaringType, "Failed to parse config file. Check your .config file is well formed XML.", confEx);
                }
                else
                {
                    // This exception is typically due to the assembly name not being correctly specified in the section type.
                    string configSectionStr = "<section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler," + Assembly.GetExecutingAssembly().FullName + "\" />";
                    LogLog.Error(declaringType, "Failed to parse config file. Is the <configSections> specified as: " + configSectionStr, confEx);
                }
            }
        }

        /// <summary>
        /// 利用从配置文件中读取到的 XmlElement 配置 ILoggerRepository
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="element"></param>
        static private void InternalConfigureFromXml(ILoggerRepository repository, XmlElement element)
        {
            if (element == null)
            {
                LogLog.Error(declaringType, "ConfigureFromXml called with null 'element' parameter");
            }
            else if (repository == null)
            {
                LogLog.Error(declaringType, "ConfigureFromXml called with null 'repository' parameter");
            }
            else
            {
                LogLog.Debug(declaringType, "Configuring Repository [" + repository.Name + "]");

                IXmlRepositoryConfigurator configurableRepository = repository as IXmlRepositoryConfigurator;
                if (configurableRepository == null)
                {
                    LogLog.Warn(declaringType, "Repository [" + repository + "] does not support the XmlConfigurator");
                }
                else
                {
                    // Copy the xml data into the root of a new document
                    // this isolates the xml config data from the rest of
                    // the document
                    XmlDocument newDoc = new XmlDocument();
                    XmlElement newElement = (XmlElement)newDoc.AppendChild(newDoc.ImportNode(element, true));

                    // Pass the configurator the config element
                    configurableRepository.Configure(newElement);
                }
            }
        }

        static private void InternalConfigure(ILoggerRepository repository, FileInfo configFile)
        {
            LogLog.Debug(declaringType, "configuring repository [" + repository.Name + "] using file [" + configFile + "]");

            if (configFile == null)
            {
                LogLog.Error(declaringType, "Configure called with null 'configFile' parameter");
            }
            else
            {
                // Have to use File.Exists() rather than configFile.Exists()
                // because configFile.Exists() caches the value, not what we want.
                if (File.Exists(configFile.FullName))
                {
                    // Open the file for reading
                    FileStream fs = null;

                    // Try hard to open the file
                    for (int retry = 5; --retry >= 0;)
                    {
                        try
                        {
                            fs = configFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                            break;
                        }
                        catch (IOException ex)
                        {
                            if (retry == 0)
                            {
                                LogLog.Error(declaringType, "Failed to open XML config file [" + configFile.Name + "]", ex);

                                // The stream cannot be valid
                                fs = null;
                            }
                            System.Threading.Thread.Sleep(250);
                        }
                    }

                    if (fs != null)
                    {
                        try
                        {
                            // Load the configuration from the stream
                            InternalConfigure(repository, fs);
                        }
                        finally
                        {
                            // Force the file closed whatever happens
                            fs.Close();
                        }
                    }
                }
                else
                {
                    LogLog.Debug(declaringType, "config file [" + configFile.FullName + "] not found. Configuration unchanged.");
                }
            }
        }

        static private void InternalConfigure(ILoggerRepository repository, Stream configStream)
        {
            LogLog.Debug(declaringType, "configuring repository [" + repository.Name + "] using stream");

            if (configStream == null)
            {
                LogLog.Error(declaringType, "Configure called with null 'configStream' parameter");
            }
            else
            {
                // Load the config file into a document
                XmlDocument doc = new XmlDocument();
                try
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Parse;

                    XmlReader xmlReader = XmlReader.Create(configStream, settings);

                    // load the data into the document
                    doc.Load(xmlReader);
                }
                catch (Exception ex)
                {
                    LogLog.Error(declaringType, "Error while loading XML configuration", ex);

                    // The document is invalid
                    doc = null;
                }

                if (doc != null)
                {
                    LogLog.Debug(declaringType, "loading XML configuration");

                    // Configure using the 'log4net' element
                    XmlNodeList configNodeList = doc.GetElementsByTagName("log4net");
                    if (configNodeList.Count == 0)
                    {
                        LogLog.Debug(declaringType, "XML configuration does not contain a <log4net> element. Configuration Aborted.");
                    }
                    else if (configNodeList.Count > 1)
                    {
                        LogLog.Error(declaringType, "XML configuration contains [" + configNodeList.Count + "] <log4net> elements. Only one is allowed. Configuration Aborted.");
                    }
                    else
                    {
                        InternalConfigureFromXml(repository, configNodeList[0] as XmlElement);
                    }
                }
            }
        }

        static private void InternalConfigure(ILoggerRepository repository, Uri configUri)
        {
            LogLog.Debug(declaringType, "configuring repository [" + repository.Name + "] using URI [" + configUri + "]");

            if (configUri == null)
            {
                LogLog.Error(declaringType, "Configure called with null 'configUri' parameter");
            }
            else
            {
                if (configUri.IsFile)
                {
                    // If URI is local file then call Configure with FileInfo
                    InternalConfigure(repository, new FileInfo(configUri.LocalPath));
                }
                else
                {
                    // NETCF dose not support WebClient
                    WebRequest configRequest = null;

                    try
                    {
                        configRequest = WebRequest.Create(configUri);
                    }
                    catch (Exception ex)
                    {
                        LogLog.Error(declaringType, "Failed to create WebRequest for URI [" + configUri + "]", ex);
                    }

                    if (configRequest != null)
                    {
                        // authentication may be required, set client to use default credentials
                        try
                        {
                            configRequest.Credentials = CredentialCache.DefaultCredentials;
                        }
                        catch
                        {
                            // ignore security exception
                        }

                        try
                        {
                            WebResponse response = configRequest.GetResponse();

                            if (response != null)
                            {
                                try
                                {
                                    // Open stream on config URI
                                    using (Stream configStream = response.GetResponseStream())
                                    {
                                        InternalConfigure(repository, configStream);
                                    }
                                }
                                finally
                                {
                                    response.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(declaringType, "Failed to request config from URI [" + configUri + "]", ex);
                        }
                    }
                }
            }
        }

        #endregion Private Static Methods

        #region ConfigureAndWatch static methods

        static public ICollection ConfigureAndWatch(FileInfo configFile)
        {
            ArrayList configurationMessages = new ArrayList();

            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigureAndWatch(repository, configFile);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        static public ICollection ConfigureAndWatch(ILoggerRepository repository, FileInfo configFile)
        {
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                InternalConfigureAndWatch(repository, configFile);
            }

            repository.ConfigurationMessages = configurationMessages;

            return configurationMessages;
        }

        #endregion

        #region Private ConfigureAndWatch static methods

        static private void InternalConfigureAndWatch(ILoggerRepository repository, FileInfo configFile)
        {
            LogLog.Debug(declaringType, "configuring repository [" + repository.Name + "] using file [" + configFile + "] watching for file updates");

            if (configFile == null)
            {
                LogLog.Error(declaringType, "ConfigureAndWatch called with null 'configFile' parameter");
            }
            else
            {
                // Configure log4net now
                InternalConfigure(repository, configFile);

                try
                {
                    lock (m_repositoryName2ConfigAndWatchHandler)
                    {
                        // support multiple repositories each having their own watcher
                        ConfigureAndWatchHandler handler =
                            (ConfigureAndWatchHandler)m_repositoryName2ConfigAndWatchHandler[configFile.FullName];

                        if (handler != null)
                        {
                            m_repositoryName2ConfigAndWatchHandler.Remove(configFile.FullName);
                            handler.Dispose();
                        }

                        // Create and start a watch handler that will reload the
                        // configuration whenever the config file is modified.
                        handler = new ConfigureAndWatchHandler(repository, configFile);
                        m_repositoryName2ConfigAndWatchHandler[configFile.FullName] = handler;
                    }
                }
                catch (Exception ex)
                {
                    LogLog.Error(declaringType, "Failed to initialize configuration file watcher for file [" + configFile.FullName + "]", ex);
                }
            }
        }

        private sealed class ConfigureAndWatchHandler : IDisposable
        {
            /// <summary>
            /// Holds the FileInfo used to configure the XmlConfigurator
            /// </summary>
            private FileInfo m_configFile;

            /// <summary>
            /// Holds the repository being configured.
            /// </summary>
            private ILoggerRepository m_repository;

            /// <summary>
            /// The timer used to compress the notification events.
            /// </summary>
            private Timer m_timer;

            /// <summary>
            /// The default amount of time to wait after receiving notification
            /// before reloading the config file.
            /// </summary>
            private const int TimeoutMillis = 500;

            /// <summary>
            /// Watches file for changes. This object should be disposed when no longer
            /// needed to free system handles on the watched resources.
            /// </summary>
            private FileSystemWatcher m_watcher;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfigureAndWatchHandler" /> class to
            /// watch a specified config file used to configure a repository.
            /// </summary>
            /// <param name="repository">The repository to configure.</param>
            /// <param name="configFile">The configuration file to watch.</param>
            /// <remarks>
            /// <para>
            /// Initializes a new instance of the <see cref="ConfigureAndWatchHandler" /> class.
            /// </para>
            /// </remarks>
            [System.Security.SecuritySafeCritical]
            public ConfigureAndWatchHandler(ILoggerRepository repository, FileInfo configFile)
            {
                m_repository = repository;
                m_configFile = configFile;

                // Create a new FileSystemWatcher and set its properties.
                m_watcher = new FileSystemWatcher();

                m_watcher.Path = m_configFile.DirectoryName;
                m_watcher.Filter = m_configFile.Name;

                // Set the notification filters
                m_watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;

                // Add event handlers. OnChanged will do for all event handlers that fire a FileSystemEventArgs
                m_watcher.Changed += new FileSystemEventHandler(ConfigureAndWatchHandler_OnChanged);
                m_watcher.Created += new FileSystemEventHandler(ConfigureAndWatchHandler_OnChanged);
                m_watcher.Deleted += new FileSystemEventHandler(ConfigureAndWatchHandler_OnChanged);
                m_watcher.Renamed += new RenamedEventHandler(ConfigureAndWatchHandler_OnRenamed);

                // Begin watching.
                m_watcher.EnableRaisingEvents = true;

                // Create the timer that will be used to deliver events. Set as disabled
                m_timer = new Timer(new TimerCallback(OnWatchedFileChange), null, Timeout.Infinite, Timeout.Infinite);
            }

            /// <summary>
            /// Event handler used by <see cref="ConfigureAndWatchHandler"/>.
            /// </summary>
            /// <param name="source">The <see cref="FileSystemWatcher"/> firing the event.</param>
            /// <param name="e">The argument indicates the file that caused the event to be fired.</param>
            /// <remarks>
            /// <para>
            /// This handler reloads the configuration from the file when the event is fired.
            /// </para>
            /// </remarks>
            private void ConfigureAndWatchHandler_OnChanged(object source, FileSystemEventArgs e)
            {
                LogLog.Debug(declaringType, "ConfigureAndWatchHandler: " + e.ChangeType + " [" + m_configFile.FullName + "]");

                // Deliver the event in TimeoutMillis time
                // timer will fire only once
                m_timer.Change(TimeoutMillis, Timeout.Infinite);
            }

            /// <summary>
            /// Event handler used by <see cref="ConfigureAndWatchHandler"/>.
            /// </summary>
            /// <param name="source">The <see cref="FileSystemWatcher"/> firing the event.</param>
            /// <param name="e">The argument indicates the file that caused the event to be fired.</param>
            /// <remarks>
            /// <para>
            /// This handler reloads the configuration from the file when the event is fired.
            /// </para>
            /// </remarks>
            private void ConfigureAndWatchHandler_OnRenamed(object source, RenamedEventArgs e)
            {
                LogLog.Debug(declaringType, "ConfigureAndWatchHandler: " + e.ChangeType + " [" + m_configFile.FullName + "]");

                // Deliver the event in TimeoutMillis time
                // timer will fire only once
                m_timer.Change(TimeoutMillis, Timeout.Infinite);
            }

            /// <summary>
            /// Called by the timer when the configuration has been updated.
            /// </summary>
            /// <param name="state">null</param>
            private void OnWatchedFileChange(object state)
            {
                XmlConfigurator.InternalConfigure(m_repository, m_configFile);
            }

            /// <summary>
            /// Release the handles held by the watcher and timer.
            /// </summary>
            [System.Security.SecuritySafeCritical]
            public void Dispose()
            {
                m_watcher.EnableRaisingEvents = false;
                m_watcher.Dispose();
                m_timer.Dispose();
            }
        }

        #endregion

        private readonly static Hashtable m_repositoryName2ConfigAndWatchHandler = new Hashtable();
        private readonly static Type declaringType = typeof(XmlConfigurator);
    }
}
