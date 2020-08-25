using Log4NetDemo.Configration;
using Log4NetDemo.Configration.Attributes;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Log4NetDemo.Repository
{
    class DefaultRepositorySelector : IRepositorySelector
    {
        public DefaultRepositorySelector(Type defaultRepositoryType)
        {
            if (defaultRepositoryType == null)
            {
                throw new ArgumentNullException("defaultRepositoryType");
            }

            // Check that the type is a repository
            if (!(typeof(ILoggerRepository).IsAssignableFrom(defaultRepositoryType)))
            {
                throw SystemInfo.CreateArgumentOutOfRangeException("defaultRepositoryType", defaultRepositoryType, "Parameter: defaultRepositoryType, Value: [" + defaultRepositoryType + "] out of range. Argument must implement the ILoggerRepository interface");
            }

            m_defaultRepositoryType = defaultRepositoryType;

            LogLog.Debug(declaringType, "defaultRepositoryType [" + m_defaultRepositoryType + "]");
        }

        #region Implementation of IRepositorySelector

        public event LoggerRepositoryCreationEventHandler LoggerRepositoryCreatedEvent
        {
            add { m_loggerRepositoryCreatedEvent += value; }
            remove { m_loggerRepositoryCreatedEvent -= value; }
        }

        public ILoggerRepository GetRepository(Assembly repositoryAssembly)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            return CreateRepository(repositoryAssembly, m_defaultRepositoryType);
        }

        public ILoggerRepository GetRepository(string repositoryName)
        {
            if (repositoryName == null)
            {
                throw new ArgumentNullException("repositoryName");
            }

            lock (this)
            {
                // Lookup in map
                ILoggerRepository rep = m_name2repositoryMap[repositoryName] as ILoggerRepository;
                if (rep == null)
                {
                    throw new LogException("Repository [" + repositoryName + "] is NOT defined.");
                }
                return rep;
            }
        }

        public ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType)
        {
            return CreateRepository(repositoryAssembly, repositoryType, DefaultRepositoryName, true);
        }

        public ILoggerRepository CreateRepository(string repositoryName, Type repositoryType)
        {
            if (repositoryName == null)
            {
                throw new ArgumentNullException("repositoryName");
            }

            // If the type is not set then use the default type
            if (repositoryType == null)
            {
                repositoryType = m_defaultRepositoryType;
            }

            lock (this)
            {
                ILoggerRepository rep = null;

                // First check that the repository does not exist
                rep = m_name2repositoryMap[repositoryName] as ILoggerRepository;
                if (rep != null)
                {
                    throw new LogException("Repository [" + repositoryName + "] is already defined. Repositories cannot be redefined.");
                }
                else
                {
                    // Lookup an alias before trying to create the new repository
                    ILoggerRepository aliasedRepository = m_alias2repositoryMap[repositoryName] as ILoggerRepository;
                    if (aliasedRepository != null)
                    {
                        // Found an alias

                        // Check repository type
                        if (aliasedRepository.GetType() == repositoryType)
                        {
                            // Repository type is compatible
                            LogLog.Debug(declaringType, "Aliasing repository [" + repositoryName + "] to existing repository [" + aliasedRepository.Name + "]");
                            rep = aliasedRepository;

                            // Store in map
                            m_name2repositoryMap[repositoryName] = rep;
                        }
                        else
                        {
                            // Invalid repository type for alias
                            LogLog.Error(declaringType, "Failed to alias repository [" + repositoryName + "] to existing repository [" + aliasedRepository.Name + "]. Requested repository type [" + repositoryType.FullName + "] is not compatible with existing type [" + aliasedRepository.GetType().FullName + "]");

                            // We now drop through to create the repository without aliasing
                        }
                    }

                    // If we could not find an alias
                    if (rep == null)
                    {
                        LogLog.Debug(declaringType, "Creating repository [" + repositoryName + "] using type [" + repositoryType + "]");

                        // Call the no arg constructor for the repositoryType
                        rep = (ILoggerRepository)Activator.CreateInstance(repositoryType);

                        // Set the name of the repository
                        rep.Name = repositoryName;

                        // Store in map
                        m_name2repositoryMap[repositoryName] = rep;

                        // Notify listeners that the repository has been created
                        OnLoggerRepositoryCreatedEvent(rep);
                    }
                }

                return rep;
            }
        }

        public bool ExistsRepository(string repositoryName)
        {
            lock (this)
            {
                return m_name2repositoryMap.ContainsKey(repositoryName);
            }
        }

        public ILoggerRepository[] GetAllRepositories()
        {
            lock (this)
            {
                ICollection reps = m_name2repositoryMap.Values;
                ILoggerRepository[] all = new ILoggerRepository[reps.Count];
                reps.CopyTo(all, 0);
                return all;
            }
        }

        #endregion

        /// <summary>
        /// 为程序集创建新的 ILoggerRepository
        /// </summary>
        /// <param name="repositoryAssembly"></param>
        /// <param name="repositoryType">要创建的 ILoggerRepository 的具体类型， must implement <see cref="ILoggerRepository"/>.</param>
        /// <param name="repositoryName"></param>
        /// <param name="readAssemblyAttributes">是否读取程序集的特性</param>
        /// <returns></returns>
        public ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType, string repositoryName, bool readAssemblyAttributes)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }

            // If the type is not set then use the default type
            if (repositoryType == null)
            {
                repositoryType = m_defaultRepositoryType;
            }

            lock (this)
            {
                // Lookup in map
                ILoggerRepository rep = m_assembly2repositoryMap[repositoryAssembly] as ILoggerRepository;
                if (rep == null)
                {
                    // Not found, therefore create
                    LogLog.Debug(declaringType, "Creating repository for assembly [" + repositoryAssembly + "]");

                    // Must specify defaults
                    string actualRepositoryName = repositoryName;
                    Type actualRepositoryType = repositoryType;

                    if (readAssemblyAttributes)
                    {
                        // Get the repository and type from the assembly attributes
                        GetInfoForAssembly(repositoryAssembly, ref actualRepositoryName, ref actualRepositoryType);
                    }

                    LogLog.Debug(declaringType, "Assembly [" + repositoryAssembly + "] using repository [" + actualRepositoryName + "] and repository type [" + actualRepositoryType + "]");

                    // Lookup the repository in the map (as this may already be defined)
                    rep = m_name2repositoryMap[actualRepositoryName] as ILoggerRepository;
                    if (rep == null)
                    {
                        // Create the repository
                        rep = CreateRepository(actualRepositoryName, actualRepositoryType);

                        if (readAssemblyAttributes)
                        {
                            try
                            {
                                // Look for aliasing attributes
                                LoadAliases(repositoryAssembly, rep);

                                // Look for plugins defined on the assembly
                                //LoadPlugins(repositoryAssembly, rep);

                                // Configure the repository using the assembly attributes
                                ConfigureRepository(repositoryAssembly, rep);
                            }
                            catch (Exception ex)
                            {
                                LogLog.Error(declaringType, "Failed to configure repository [" + actualRepositoryName + "] from assembly attributes.", ex);
                            }
                        }
                    }
                    else
                    {
                        LogLog.Debug(declaringType, "repository [" + actualRepositoryName + "] already exists, using repository type [" + rep.GetType().FullName + "]");

                        if (readAssemblyAttributes)
                        {
                            try
                            {
                                // Look for plugins defined on the assembly
                                //LoadPlugins(repositoryAssembly, rep);
                            }
                            catch (Exception ex)
                            {
                                LogLog.Error(declaringType, "Failed to configure repository [" + actualRepositoryName + "] from assembly attributes.", ex);
                            }
                        }
                    }
                    m_assembly2repositoryMap[repositoryAssembly] = rep;
                }
                return rep;
            }
        }

        /// <summary>
        /// 给已经存在的 ILoggerRepository 设置一个别名
        /// </summary>
        /// <param name="repositoryAlias"></param>
        /// <param name="repositoryTarget"></param>
        public void AliasRepository(string repositoryAlias, ILoggerRepository repositoryTarget)
        {
            if (repositoryAlias == null)
            {
                throw new ArgumentNullException("repositoryAlias");
            }
            if (repositoryTarget == null)
            {
                throw new ArgumentNullException("repositoryTarget");
            }

            lock (this)
            {
                // Check if the alias is already set
                if (m_alias2repositoryMap.Contains(repositoryAlias))
                {
                    // Check if this is a duplicate of the current alias
                    if (repositoryTarget != ((ILoggerRepository)m_alias2repositoryMap[repositoryAlias]))
                    {
                        // Cannot redefine existing alias
                        throw new InvalidOperationException("Repository [" + repositoryAlias + "] is already aliased to repository [" + ((ILoggerRepository)m_alias2repositoryMap[repositoryAlias]).Name + "]. Aliases cannot be redefined.");
                    }
                }
                // Check if the alias is already mapped to a repository
                else if (m_name2repositoryMap.Contains(repositoryAlias))
                {
                    // Check if this is a duplicate of the current mapping
                    if (repositoryTarget != ((ILoggerRepository)m_name2repositoryMap[repositoryAlias]))
                    {
                        // Cannot define alias for already mapped repository
                        throw new InvalidOperationException("Repository [" + repositoryAlias + "] already exists and cannot be aliased to repository [" + repositoryTarget.Name + "].");
                    }
                }
                else
                {
                    // Set the alias
                    m_alias2repositoryMap[repositoryAlias] = repositoryTarget;
                }
            }
        }

        protected virtual void OnLoggerRepositoryCreatedEvent(ILoggerRepository repository)
        {
            m_loggerRepositoryCreatedEvent?.Invoke(this, new LoggerRepositoryCreationEventArgs(repository));
        }

        #region Private Instance Methods

        /// <summary>
        /// 从程序集的特性中获取 repositoryName 和 repositoryType
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="repositoryName"></param>
        /// <param name="repositoryType"></param>
        private void GetInfoForAssembly(Assembly assembly, ref string repositoryName, ref Type repositoryType)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            try
            {
                LogLog.Debug(declaringType, "Assembly [" + assembly.FullName + "] Loaded From [" + SystemInfo.AssemblyLocationInfo(assembly) + "]");
            }
            catch
            {
                // Ignore exception from debug call
            }

            try
            {
                // Look for the RepositoryAttribute on the assembly 
                object[] repositoryAttributes = Attribute.GetCustomAttributes(assembly, typeof(RepositoryAttribute), false);
                if (repositoryAttributes == null || repositoryAttributes.Length == 0)
                {
                    // This is not a problem, but its nice to know what is going on.
                    LogLog.Debug(declaringType, "Assembly [" + assembly + "] does not have a RepositoryAttribute specified.");
                }
                else
                {
                    if (repositoryAttributes.Length > 1)
                    {
                        LogLog.Error(declaringType, "Assembly [" + assembly + "] has multiple log4net.Config.RepositoryAttribute assembly attributes. Only using first occurrence.");
                    }

                    RepositoryAttribute domAttr = repositoryAttributes[0] as RepositoryAttribute;

                    if (domAttr == null)
                    {
                        LogLog.Error(declaringType, "Assembly [" + assembly + "] has a RepositoryAttribute but it does not!.");
                    }
                    else
                    {
                        // If the Name property is set then override the default
                        if (domAttr.Name != null)
                        {
                            repositoryName = domAttr.Name;
                        }

                        // If the RepositoryType property is set then override the default
                        if (domAttr.RepositoryType != null)
                        {
                            // Check that the type is a repository
                            if (typeof(ILoggerRepository).IsAssignableFrom(domAttr.RepositoryType))
                            {
                                repositoryType = domAttr.RepositoryType;
                            }
                            else
                            {
                                LogLog.Error(declaringType, "DefaultRepositorySelector: Repository Type [" + domAttr.RepositoryType + "] must implement the ILoggerRepository interface.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Unhandled exception in GetInfoForAssembly", ex);
            }
        }

        /// <summary>
        /// 使用程序集中的信息 配置 ILoggerRepository
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="repository"></param>
        private void ConfigureRepository(Assembly assembly, ILoggerRepository repository)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            // Look for the Configurator attributes (e.g. XmlConfiguratorAttribute) on the assembly
            object[] configAttributes = Attribute.GetCustomAttributes(assembly, typeof(ConfiguratorAttribute), false);
            if (configAttributes != null && configAttributes.Length > 0)
            {
                // Sort the ConfiguratorAttributes in priority order
                Array.Sort(configAttributes);

                // Delegate to the attribute the job of configuring the repository
                foreach (ConfiguratorAttribute configAttr in configAttributes)
                {
                    if (configAttr != null)
                    {
                        try
                        {
                            configAttr.Configure(assembly, repository);
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(declaringType, "Exception calling [" + configAttr.GetType().FullName + "] .Configure method.", ex);
                        }
                    }
                }
            }

            if (repository.Name == DefaultRepositoryName)
            {
                // Try to configure the default repository using an AppSettings specified config file
                // Do this even if the repository has been configured (or claims to be), this allows overriding
                // of the default config files etc, if that is required.

                // 如果时默认库，就读取 AppSettings 下的默认配置文件，覆盖掉之前设置的

                string repositoryConfigFile = SystemInfo.GetAppSetting("log4net.Config");
                if (repositoryConfigFile != null && repositoryConfigFile.Length > 0)
                {
                    string applicationBaseDirectory = null;
                    try
                    {
                        applicationBaseDirectory = SystemInfo.ApplicationBaseDirectory;
                    }
                    catch (Exception ex)
                    {
                        LogLog.Warn(declaringType, "Exception getting ApplicationBaseDirectory. appSettings log4net.Config path [" + repositoryConfigFile + "] will be treated as an absolute URI", ex);
                    }

                    string repositoryConfigFilePath = repositoryConfigFile;
                    if (applicationBaseDirectory != null)
                    {
                        repositoryConfigFilePath = Path.Combine(applicationBaseDirectory, repositoryConfigFile);
                    }

                    // Determine whether to watch the file or not based on an app setting value:
                    // 根据应用程序设置值确定是否监视文件
                    bool watchRepositoryConfigFile = false;
                    Boolean.TryParse(SystemInfo.GetAppSetting("log4net.Config.Watch"), out watchRepositoryConfigFile);

                    if (watchRepositoryConfigFile)
                    {
                        // As we are going to watch the config file it is required to resolve it as a 
                        // physical file system path pass that in a FileInfo object to the Configurator
                        FileInfo repositoryConfigFileInfo = null;
                        try
                        {
                            repositoryConfigFileInfo = new FileInfo(repositoryConfigFilePath);
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(declaringType, "DefaultRepositorySelector: Exception while parsing log4net.Config file physical path [" + repositoryConfigFilePath + "]", ex);
                        }
                        try
                        {
                            LogLog.Debug(declaringType, "Loading and watching configuration for default repository from AppSettings specified Config path [" + repositoryConfigFilePath + "]");

                            XmlConfigurator.ConfigureAndWatch(repository, repositoryConfigFileInfo);
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(declaringType, "DefaultRepositorySelector: Exception calling XmlConfigurator.ConfigureAndWatch method with ConfigFilePath [" + repositoryConfigFilePath + "]", ex);
                        }
                    }
                    else
                    {
                        // As we are not going to watch the config file it is easiest to just resolve it as a 
                        // URI and pass that to the Configurator
                        Uri repositoryConfigUri = null;
                        try
                        {
                            repositoryConfigUri = new Uri(repositoryConfigFilePath);
                        }
                        catch (Exception ex)
                        {
                            LogLog.Error(declaringType, "Exception while parsing log4net.Config file path [" + repositoryConfigFile + "]", ex);
                        }

                        if (repositoryConfigUri != null)
                        {
                            LogLog.Debug(declaringType, "Loading configuration for default repository from AppSettings specified Config URI [" + repositoryConfigUri.ToString() + "]");

                            try
                            {
                                // TODO: Support other types of configurator
                                XmlConfigurator.Configure(repository, repositoryConfigUri);
                            }
                            catch (Exception ex)
                            {
                                LogLog.Error(declaringType, "Exception calling XmlConfigurator.Configure method with ConfigUri [" + repositoryConfigUri + "]", ex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从程序集特征中设置 ILoggerRepository 别名
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="repository"></param>
        private void LoadAliases(Assembly assembly, ILoggerRepository repository)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            // Look for the AliasRepositoryAttribute on the assembly
            object[] configAttributes = Attribute.GetCustomAttributes(assembly, typeof(AliasRepositoryAttribute), false);
            if (configAttributes != null && configAttributes.Length > 0)
            {
                foreach (AliasRepositoryAttribute configAttr in configAttributes)
                {
                    try
                    {
                        AliasRepository(configAttr.Name, repository);
                    }
                    catch (Exception ex)
                    {
                        LogLog.Error(declaringType, "Failed to alias repository [" + configAttr.Name + "]", ex);
                    }
                }
            }
        }

        #endregion


        private readonly static Type declaringType = typeof(DefaultRepositorySelector);
        private const string DefaultRepositoryName = "log4net-default-repository";

        private readonly Hashtable m_name2repositoryMap = new Hashtable();
        private readonly Hashtable m_assembly2repositoryMap = new Hashtable();
        private readonly Hashtable m_alias2repositoryMap = new Hashtable();
        private readonly Type m_defaultRepositoryType;

        private event LoggerRepositoryCreationEventHandler m_loggerRepositoryCreatedEvent;
    }

    public class LogException : ApplicationException
    {
        public LogException()
        {
        }

        public LogException(String message) : base(message)
        {
        }

        public LogException(String message, Exception innerException) : base(message, innerException)
        {
        }

        protected LogException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
