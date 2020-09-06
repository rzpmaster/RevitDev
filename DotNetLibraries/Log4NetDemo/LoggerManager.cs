using System;
using System.Reflection;
using Log4NetDemo.Core;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;

namespace Log4NetDemo
{
    /// <summary>
    /// 控制 ILoggerRepository 的创建,从而控制 ILog 创建
    /// </summary>
    /// <remarks>
    /// <para>This class is used by the wrapper managers (e.g. <see cref="Core.LogManager"/>)
	/// to provide access to the <see cref="ILogger"/> objects.</para>
    /// <para>This manager also holds the <see cref="IRepositorySelector"/> that is used to
	/// lookup and create repositories. The selector can be set either programmatically using
	/// the <see cref="IRepositorySelector"/> property, or by setting the <c>RepositorySelector</c> AppSetting in the applications config file to the fully qualified type name of the
	/// selector to use. </para>
    /// </remarks>
    public sealed class LoggerManager
    {
        /// <summary>
        /// 内部所有的方法都是通过这个 IRepositorySelector 请求 IRepository ,然后在通过仓库请求 ILog 。
        /// </summary>
        private static IRepositorySelector s_repositorySelector;

        private LoggerManager() { }

        /// <summary>
        /// 注册 关闭 事件
        /// </summary>
        static LoggerManager()
        {
            try
            {
                RegisterAppDomainEvents();
            }
            catch (System.Security.SecurityException)
            {
                LogLog.Debug(declaringType, "Security Exception (ControlAppDomain LinkDemand) while trying " +
                    "to register Shutdown handler with the AppDomain. LoggerManager.Shutdown() " +
                    "will not be called automatically when the AppDomain exits. It must be called " +
                    "programmatically.");
            }

            LogLog.Debug(declaringType, GetVersionInfo());

            string appRepositorySelectorTypeName = SystemInfo.GetAppSetting("log4net.RepositorySelector");
            if (appRepositorySelectorTypeName != null && appRepositorySelectorTypeName.Length > 0)
            {
                // Resolve the config string into a Type
                Type appRepositorySelectorType = null;
                try
                {
                    appRepositorySelectorType = SystemInfo.GetTypeFromString(appRepositorySelectorTypeName, false, true);
                }
                catch (Exception ex)
                {
                    LogLog.Error(declaringType, "Exception while resolving RepositorySelector Type [" + appRepositorySelectorTypeName + "]", ex);
                }

                if (appRepositorySelectorType != null)
                {
                    // Create an instance of the RepositorySelectorType
                    object appRepositorySelectorObj = null;
                    try
                    {
                        appRepositorySelectorObj = Activator.CreateInstance(appRepositorySelectorType);
                    }
                    catch (Exception ex)
                    {
                        LogLog.Error(declaringType, "Exception while creating RepositorySelector [" + appRepositorySelectorType.FullName + "]", ex);
                    }

                    if (appRepositorySelectorObj != null && appRepositorySelectorObj is IRepositorySelector)
                    {
                        s_repositorySelector = (IRepositorySelector)appRepositorySelectorObj;
                    }
                    else
                    {
                        LogLog.Error(declaringType, "RepositorySelector Type [" + appRepositorySelectorType.FullName + "] is not an IRepositorySelector");
                    }
                }
            }

            if (s_repositorySelector == null)
            {
                s_repositorySelector = new DefaultRepositorySelector(typeof(Repository.Hierarchy.Hierarchy));
            }
        }

        private static void RegisterAppDomainEvents()
        {
            // ProcessExit seems to be fired if we are part of the default domain
            // 如果我们是默认域的一部分，ProcessExit会触发
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            // Otherwise DomainUnload is fired
            // 如果不是，DomainUnload会触发
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnDomainUnload);
        }

        #region Public Static Methods

        public static ILoggerRepository GetRepository(string repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return RepositorySelector.GetRepository(repository);
        }

        public static ILoggerRepository GetRepository(Assembly repositoryAssembly)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            return RepositorySelector.GetRepository(repositoryAssembly);
        }

        public static ILogger Exists(string repository, string name)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return RepositorySelector.GetRepository(repository).Exists(name);
        }

        public static ILogger Exists(Assembly repositoryAssembly, string name)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return RepositorySelector.GetRepository(repositoryAssembly).Exists(name);
        }

        public static ILogger[] GetCurrentLoggers(string repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return RepositorySelector.GetRepository(repository).GetCurrentLoggers();
        }

        public static ILogger[] GetCurrentLoggers(Assembly repositoryAssembly)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            return RepositorySelector.GetRepository(repositoryAssembly).GetCurrentLoggers();
        }

        public static ILogger GetLogger(string repository, string name)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return RepositorySelector.GetRepository(repository).GetLogger(name);
        }

        public static ILogger GetLogger(Assembly repositoryAssembly, string name)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return RepositorySelector.GetRepository(repositoryAssembly).GetLogger(name);
        }

        public static ILogger GetLogger(string repository, Type type)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return RepositorySelector.GetRepository(repository).GetLogger(type.FullName);
        }

        public static ILogger GetLogger(Assembly repositoryAssembly, Type type)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return RepositorySelector.GetRepository(repositoryAssembly).GetLogger(type.FullName);
        }

        public static void Shutdown()
        {
            foreach (ILoggerRepository repository in GetAllRepositories())
            {
                repository.Shutdown();
            }
        }

        public static void ShutdownRepository(string repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            RepositorySelector.GetRepository(repository).Shutdown();
        }

        public static void ShutdownRepository(Assembly repositoryAssembly)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            RepositorySelector.GetRepository(repositoryAssembly).Shutdown();
        }

        public static void ResetConfiguration(string repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            RepositorySelector.GetRepository(repository).ResetConfiguration();
        }

        public static void ResetConfiguration(Assembly repositoryAssembly)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            RepositorySelector.GetRepository(repositoryAssembly).ResetConfiguration();
        }

        public static ILoggerRepository CreateRepository(string repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            return RepositorySelector.CreateRepository(repository, null);
        }

        public static ILoggerRepository CreateRepository(string repository, Type repositoryType)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (repositoryType == null)
            {
                throw new ArgumentNullException("repositoryType");
            }
            return RepositorySelector.CreateRepository(repository, repositoryType);
        }

        public static ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            if (repositoryType == null)
            {
                throw new ArgumentNullException("repositoryType");
            }
            return RepositorySelector.CreateRepository(repositoryAssembly, repositoryType);
        }

        public static ILoggerRepository[] GetAllRepositories()
        {
            return RepositorySelector.GetAllRepositories();
        }

        public static IRepositorySelector RepositorySelector
        {
            get { return s_repositorySelector; }
            set { s_repositorySelector = value; }
        }

        #endregion

        #region Private Static Methods

        private static string GetVersionInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            sb.Append("log4net assembly [").Append(myAssembly.FullName).Append("]. ");
            sb.Append("Loaded from [").Append(SystemInfo.AssemblyLocationInfo(myAssembly)).Append("]. ");
            sb.Append("(.NET Runtime [").Append(Environment.Version.ToString()).Append("]");
            sb.Append(" on ").Append(Environment.OSVersion.ToString());
            sb.Append(")");

            return sb.ToString();
        }

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            Shutdown();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Shutdown();
        }

        #endregion

        private readonly static Type declaringType = typeof(LoggerManager);
    }
}
