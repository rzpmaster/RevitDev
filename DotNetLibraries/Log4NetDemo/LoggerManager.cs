using Log4NetDemo.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly static Type declaringType = typeof(LoggerManager);
        private static IRepositorySelector s_repositorySelector;
    }
}
