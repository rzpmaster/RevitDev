using Log4NetDemo.Core.Interface;

namespace Log4NetDemo.Core
{
    public abstract class LoggerWrapperImpl : ILoggerWrapper
    {
        protected LoggerWrapperImpl(ILogger logger)
        {
            m_logger = logger;
        }

        #region Implementation of ILoggerWrapper

        virtual public ILogger Logger
        {
            get { return m_logger; }
        }

        #endregion

        private readonly ILogger m_logger;
    }
}
