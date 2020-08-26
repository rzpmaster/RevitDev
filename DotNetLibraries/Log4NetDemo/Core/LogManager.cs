using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Repository;
using System;
using System.Reflection;

namespace Log4NetDemo.Core
{
    /// <summary>
    /// 用于客户端请求 logger 实例
    /// </summary>
    /// <remarks>
    /// <para>This class has static methods that are used by a client to request
	/// a logger instance. The <see cref="GetLogger(string)"/> method is 
	/// used to retrieve a logger.</para>
    /// </remarks>
    /// <example>Simple example of logging messages
	/// <code lang="C#">
	/// ILog log = LogManager.GetLogger("application-log");
	/// 
	/// log.Info("Application Start");
	/// log.Debug("This is a debug message");
	/// 
	/// if (log.IsDebugEnabled)
	/// {
	///		log.Debug("This is another debug message");
	/// }
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="true" />
    public sealed class LogManager
    {
        /// <summary>
        /// 内部所有方法基本都是通过 LoggerManager 调用的，LoggerManager 是通过 WrapperMap 得到的
        /// </summary>
        private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));

        private static ILoggerWrapper WrapperCreationHandler(ILogger logger)
        {
            return new LogImpl(logger);
        }

        private LogManager() { }

        #region Type Specific Manager Methods

        public static ILog Exists(string name)
        {
            return Exists(Assembly.GetCallingAssembly(), name);
        }

        public static ILog Exists(Assembly repositoryAssembly, string name)
        {
            return WrapLogger(LoggerManager.Exists(repositoryAssembly, name));
        }

        public static ILog Exists(string repository, string name)
        {
            return WrapLogger(LoggerManager.Exists(repository, name));
        }

        public static ILog[] GetCurrentLoggers()
        {
            return GetCurrentLoggers(Assembly.GetCallingAssembly());
        }

        public static ILog[] GetCurrentLoggers(string repository)
        {
            return WrapLoggers(LoggerManager.GetCurrentLoggers(repository));
        }

        public static ILog[] GetCurrentLoggers(Assembly repositoryAssembly)
        {
            return WrapLoggers(LoggerManager.GetCurrentLoggers(repositoryAssembly));
        }

        public static ILog GetLogger(string name)
        {
            return GetLogger(Assembly.GetCallingAssembly(), name);
        }

        public static ILog GetLogger(string repository, string name)
        {
            return WrapLogger(LoggerManager.GetLogger(repository, name));
        }

        public static ILog GetLogger(Assembly repositoryAssembly, string name)
        {
            return WrapLogger(LoggerManager.GetLogger(repositoryAssembly, name));
        }

        public static ILog GetLogger(Type type)
        {
            return GetLogger(Assembly.GetCallingAssembly(), type.FullName);
        }

        public static ILog GetLogger(string repository, Type type)
        {
            return WrapLogger(LoggerManager.GetLogger(repository, type));
        }

        public static ILog GetLogger(Assembly repositoryAssembly, Type type)
        {
            return WrapLogger(LoggerManager.GetLogger(repositoryAssembly, type));
        }

        #endregion

        #region Domain & Repository Manager Methods

        public static void Shutdown()
        {
            LoggerManager.Shutdown();
        }

        public static void ShutdownRepository()
        {
            ShutdownRepository(Assembly.GetCallingAssembly());
        }

        public static void ShutdownRepository(string repository)
        {
            LoggerManager.ShutdownRepository(repository);
        }

        public static void ShutdownRepository(Assembly repositoryAssembly)
        {
            LoggerManager.ShutdownRepository(repositoryAssembly);
        }

        public static void ResetConfiguration()
        {
            ResetConfiguration(Assembly.GetCallingAssembly());
        }

        public static void ResetConfiguration(string repository)
        {
            LoggerManager.ResetConfiguration(repository);
        }

        public static void ResetConfiguration(Assembly repositoryAssembly)
        {
            LoggerManager.ResetConfiguration(repositoryAssembly);
        }

        public static ILoggerRepository GetRepository()
        {
            return GetRepository(Assembly.GetCallingAssembly());
        }

        public static ILoggerRepository GetRepository(string repository)
        {
            return LoggerManager.GetRepository(repository);
        }

        public static ILoggerRepository GetRepository(Assembly repositoryAssembly)
        {
            return LoggerManager.GetRepository(repositoryAssembly);
        }

        public static ILoggerRepository CreateRepository(Type repositoryType)
        {
            return CreateRepository(Assembly.GetCallingAssembly(), repositoryType);
        }

        public static ILoggerRepository CreateRepository(string repository)
        {
            return LoggerManager.CreateRepository(repository);
        }

        public static ILoggerRepository CreateRepository(string repository, Type repositoryType)
        {
            return LoggerManager.CreateRepository(repository, repositoryType);
        }

        public static ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType)
        {
            return LoggerManager.CreateRepository(repositoryAssembly, repositoryType);
        }

        public static ILoggerRepository[] GetAllRepositories()
        {
            return LoggerManager.GetAllRepositories();
        }

        public static bool Flush(int millisecondsTimeout)
        {
            IFlushable flushableRepository = LoggerManager.GetRepository(Assembly.GetCallingAssembly()) as IFlushable;
            if (flushableRepository == null)
            {
                return false;
            }
            else
            {
                return flushableRepository.Flush(millisecondsTimeout);
            }
        }

        #endregion

        #region Extension Handlers

        private static ILog WrapLogger(ILogger logger)
        {
            return (ILog)s_wrapperMap.GetWrapper(logger);
        }

        private static ILog[] WrapLoggers(ILogger[] loggers)
        {
            ILog[] results = new ILog[loggers.Length];
            for (int i = 0; i < loggers.Length; i++)
            {
                results[i] = WrapLogger(loggers[i]);
            }
            return results;
        }

        #endregion
    }
}
