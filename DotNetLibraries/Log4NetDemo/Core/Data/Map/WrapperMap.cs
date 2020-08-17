using Log4NetDemo.Core.Interface;
using Log4NetDemo.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Core.Data.Map
{
    /// <summary>
    /// 包装器创建事件委托
    /// </summary>
    /// <param name="logger">要包装的 ILogger</param>
    /// <returns></returns>
    public delegate ILoggerWrapper WrapperCreationHandler(ILogger logger);

    /// <summary>
	/// Maps between logger objects and wrapper objects.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class maintains a mapping between <see cref="ILogger"/> objects and
	/// <see cref="ILoggerWrapper"/> objects. Use the <see cref="GetWrapper"/> method to 
	/// lookup the <see cref="ILoggerWrapper"/> for the specified <see cref="ILogger"/>.
	/// </para>
	/// <para>
	/// New wrapper instances are created by the <see cref="CreateNewWrapperObject"/>
	/// method. The default behavior is for this method to delegate construction
	/// of the wrapper to the <see cref="WrapperCreationHandler"/> delegate supplied
	/// to the constructor. This allows specialization of the behavior without
	/// requiring subclassing of this type.
	/// </para>
	/// </remarks>
    public class WrapperMap
    {
        public WrapperMap(WrapperCreationHandler createWrapperHandler)
        {
            m_createWrapperHandler = createWrapperHandler;

            // Create the delegates for the event callbacks
            m_shutdownHandler = new LoggerRepositoryShutdownEventHandler(ILoggerRepository_Shutdown);
        }

        virtual public ILoggerWrapper GetWrapper(ILogger logger)
        {
            // If the logger is null then the corresponding wrapper is null
            if (logger == null)
            {
                return null;
            }

            lock (this)
            {
                // Lookup hierarchy in map.
                Hashtable wrappersMap = (Hashtable)m_repositories[logger.Repository];

                if (wrappersMap == null)
                {
                    // Hierarchy does not exist in map.
                    // Must register with hierarchy

                    wrappersMap = new Hashtable();
                    m_repositories[logger.Repository] = wrappersMap;

                    // Register for config reset & shutdown on repository
                    logger.Repository.ShutdownEvent += m_shutdownHandler;
                }

                // Look for the wrapper object in the map
                ILoggerWrapper wrapperObject = wrappersMap[logger] as ILoggerWrapper;

                if (wrapperObject == null)
                {
                    // No wrapper object exists for the specified logger

                    // Create a new wrapper wrapping the logger
                    wrapperObject = CreateNewWrapperObject(logger);

                    // Store wrapper logger in map
                    wrappersMap[logger] = wrapperObject;
                }

                return wrapperObject;
            }
        }

        #region Protected Instance Methods

        protected Hashtable Repositories
        {
            get { return this.m_repositories; }
        }

        virtual protected ILoggerWrapper CreateNewWrapperObject(ILogger logger)
        {
            if (m_createWrapperHandler != null)
            {
                return m_createWrapperHandler(logger);
            }
            return null;
        }

        virtual protected void RepositoryShutdown(ILoggerRepository repository)
        {
            lock (this)
            {
                // Remove the repository from map
                m_repositories.Remove(repository);

                // Unhook events from the repository
                repository.ShutdownEvent -= m_shutdownHandler;
            }
        }

        private void ILoggerRepository_Shutdown(object sender, EventArgs e)
        {
            ILoggerRepository repository = sender as ILoggerRepository;
            if (repository != null)
            {
                // Remove all repository from map
                RepositoryShutdown(repository);
            }
        }

        #endregion

        /// <summary>
        /// 内部维护的哈希表
        /// </summary>
        private readonly Hashtable m_repositories = new Hashtable();
        /// <summary>
        /// 包装器创建事件
        /// </summary>
        private readonly WrapperCreationHandler m_createWrapperHandler;
        /// <summary>
        /// LoggerRepository 关闭事件
        /// </summary>
        private readonly LoggerRepositoryShutdownEventHandler m_shutdownHandler;
    }
}
