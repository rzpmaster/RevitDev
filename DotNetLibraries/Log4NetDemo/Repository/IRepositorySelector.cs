using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Repository
{
    public delegate void LoggerRepositoryCreationEventHandler(object sender, LoggerRepositoryCreationEventArgs e);

    public class LoggerRepositoryCreationEventArgs : EventArgs
    {
        private ILoggerRepository m_repository;

        public LoggerRepositoryCreationEventArgs(ILoggerRepository repository)
        {
            m_repository = repository;
        }

        public ILoggerRepository LoggerRepository
        {
            get { return m_repository; }
        }
    }

    /// <summary>
    /// LogManger 使用 IRepositorySelector 得到一个 ILoggerRepository
    /// </summary>
    public interface IRepositorySelector
    {
        /// <summary>
        /// 根据程序集查找 ILoggerRepository
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para> Assembly 和 ILoggerRepository 之间的关联尚未定义，可以关联为任何方法，但是必须相同的 Assembly 的到的相同的 ILoggerRepository</para>
        /// </remarks>
        ILoggerRepository GetRepository(Assembly assembly);

        /// <summary>
        /// 根据名称查找 
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
        /// <remarks>
		/// Lookup a named <see cref="ILoggerRepository"/>. This is the repository created by
		/// calling <see cref="CreateRepository(string,Type)"/>.
		/// </remarks>
        ILoggerRepository GetRepository(string repositoryName);

        /// <summary>
        /// 为给定程序集创建一个 ILoggerRepository
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="repositoryType">要创建的 ILoggerRepository 的类型，必须继承自 <see cref="ILoggerRepository"/>。</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>a call to <see cref="GetRepository(Assembly)"/> with the
		/// same assembly specified will return the same repository instance.</para>
        /// </remarks>
        ILoggerRepository CreateRepository(Assembly assembly, Type repositoryType);

        /// <summary>
        /// 为指定名称创建 ILoggerRepository
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <param name="repositoryType">要创建的 ILoggerRepository 的类型，必须继承自 <see cref="ILoggerRepository"/>。</param>
        /// <returns></returns>
        /// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> created will be associated with the name
		/// specified such that a call to <see cref="GetRepository(string)"/> with the
		/// same name will return the same repository instance.
		/// </para>
		/// </remarks>
        ILoggerRepository CreateRepository(string repositoryName, Type repositoryType);

        bool ExistsRepository(string repositoryName);

        ILoggerRepository[] GetAllRepositories();

        event LoggerRepositoryCreationEventHandler LoggerRepositoryCreatedEvent;
    }
}
