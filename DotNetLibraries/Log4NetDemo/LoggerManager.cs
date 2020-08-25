using Log4NetDemo.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo
{
    public sealed class LoggerManager
    {
        private LoggerManager() { }

        static LoggerManager()
        {

        }

        #region Public Static Methods

        public static ILoggerRepository GetRepository(Assembly repositoryAssembly)
        {
            if (repositoryAssembly == null)
            {
                throw new ArgumentNullException("repositoryAssembly");
            }
            return RepositorySelector.GetRepository(repositoryAssembly);
        }

        public static IRepositorySelector RepositorySelector
        {
            get { return s_repositorySelector; }
            set { s_repositorySelector = value; }
        }

        #endregion

        private static IRepositorySelector s_repositorySelector;
        private readonly static Type declaringType = typeof(LoggerManager);
    }
}
