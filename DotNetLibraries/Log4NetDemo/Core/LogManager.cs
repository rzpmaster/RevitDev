//using Log4NetDemo.Core.Data.Map;
//using Log4NetDemo.Core.Interface;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Log4NetDemo.Core
//{
//    /// <summary>
//    /// 用于客户端请求 logger 实例
//    /// </summary>
//    /// <example>Simple example of logging messages
//    /// <code lang="C#">
//    /// ILog log = LogManager.GetLogger("application-log");
//    /// 
//    /// log.Info("Application Start");
//    /// log.Debug("This is a debug message");
//    /// 
//    /// if (log.IsDebugEnabled)
//    /// {
//    ///		log.Debug("This is another debug message");
//    /// }
//    /// </code>
//    /// </example>
//    /// <threadsafety static="true" instance="true" />
//    public sealed class LogManager
//    {
//        private LogManager() { }

//        private static ILoggerWrapper WrapperCreationHandler(ILogger logger)
//        {
//            return new LogImpl(logger);
//        }

//        private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));
//    }
//}
