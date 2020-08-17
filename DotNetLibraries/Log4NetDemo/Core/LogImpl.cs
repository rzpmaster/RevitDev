using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Data.Map;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Core
{
    public class LogImpl : LoggerWrapperImpl, ILog
    {
        public LogImpl(ILogger logger) : base(logger)
        {
            // Listen for changes to the repository
            //logger.Repository.ConfigurationChanged += new LoggerRepositoryConfigurationChangedEventHandler(LoggerRepositoryConfigurationChanged);

            // load the current levels
            ReloadLevels(logger.Repository);
        }

        #region Implementation of ILog

        virtual public void Debug(object message)
        {
            Logger.Log(ThisDeclaringType, m_levelDebug, message, null);
        }

        virtual public void Debug(object message, Exception exception)
        {
            Logger.Log(ThisDeclaringType, m_levelDebug, message, exception);
        }

        virtual public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        virtual public void DebugFormat(string format, object arg0)
        {
            if (IsDebugEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
            }
        }

        virtual public void DebugFormat(string format, object arg0, object arg1)
        {
            if (IsDebugEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
            }
        }

        virtual public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsDebugEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
            }
        }

        virtual public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(provider, format, args), null);
            }
        }

        virtual public void Info(object message)
        {
            Logger.Log(ThisDeclaringType, m_levelInfo, message, null);
        }

        virtual public void Info(object message, Exception exception)
        {
            Logger.Log(ThisDeclaringType, m_levelInfo, message, exception);
        }

        virtual public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        virtual public void InfoFormat(string format, object arg0)
        {
            if (IsInfoEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
            }
        }

        virtual public void InfoFormat(string format, object arg0, object arg1)
        {
            if (IsInfoEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
            }
        }

        virtual public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsInfoEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
            }
        }

        virtual public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(provider, format, args), null);
            }
        }

        virtual public void Warn(object message)
        {
            Logger.Log(ThisDeclaringType, m_levelWarn, message, null);
        }

        virtual public void Warn(object message, Exception exception)
        {
            Logger.Log(ThisDeclaringType, m_levelWarn, message, exception);
        }

        virtual public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        virtual public void WarnFormat(string format, object arg0)
        {
            if (IsWarnEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
            }
        }

        virtual public void WarnFormat(string format, object arg0, object arg1)
        {
            if (IsWarnEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
            }
        }

        virtual public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsWarnEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
            }
        }

        virtual public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(provider, format, args), null);
            }
        }

        virtual public void Error(object message)
        {
            Logger.Log(ThisDeclaringType, m_levelError, message, null);
        }

        virtual public void Error(object message, Exception exception)
        {
            Logger.Log(ThisDeclaringType, m_levelError, message, exception);
        }

        virtual public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        virtual public void ErrorFormat(string format, object arg0)
        {
            if (IsErrorEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
            }
        }

        virtual public void ErrorFormat(string format, object arg0, object arg1)
        {
            if (IsErrorEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
            }
        }

        virtual public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsErrorEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
            }
        }

        virtual public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(provider, format, args), null);
            }
        }

        virtual public void Fatal(object message)
        {
            Logger.Log(ThisDeclaringType, m_levelFatal, message, null);
        }

        virtual public void Fatal(object message, Exception exception)
        {
            Logger.Log(ThisDeclaringType, m_levelFatal, message, exception);
        }

        virtual public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        virtual public void FatalFormat(string format, object arg0)
        {
            if (IsFatalEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
            }
        }

        virtual public void FatalFormat(string format, object arg0, object arg1)
        {
            if (IsFatalEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
            }
        }

        virtual public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsFatalEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
            }
        }

        virtual public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(provider, format, args), null);
            }
        }

        virtual public bool IsDebugEnabled
        {
            get { return Logger.IsEnabledFor(m_levelDebug); }
        }

        virtual public bool IsInfoEnabled
        {
            get { return Logger.IsEnabledFor(m_levelInfo); }
        }

        virtual public bool IsWarnEnabled
        {
            get { return Logger.IsEnabledFor(m_levelWarn); }
        }

        virtual public bool IsErrorEnabled
        {
            get { return Logger.IsEnabledFor(m_levelError); }
        }

        virtual public bool IsFatalEnabled
        {
            get { return Logger.IsEnabledFor(m_levelFatal); }
        }

        #endregion Implementation of ILog

        protected virtual void ReloadLevels(ILoggerRepository repository)
        {
            //LevelMap levelMap = repository.LevelMap;

            //m_levelDebug = levelMap.LookupWithDefault(Level.Debug);
            //m_levelInfo = levelMap.LookupWithDefault(Level.Info);
            //m_levelWarn = levelMap.LookupWithDefault(Level.Warn);
            //m_levelError = levelMap.LookupWithDefault(Level.Error);
            //m_levelFatal = levelMap.LookupWithDefault(Level.Fatal);
        }

        private void LoggerRepositoryConfigurationChanged(object sender, EventArgs e)
        {
            ILoggerRepository repository = sender as ILoggerRepository;
            if (repository != null)
            {
                ReloadLevels(repository);
            }
        }

        private Level m_levelDebug;
        private Level m_levelInfo;
        private Level m_levelWarn;
        private Level m_levelError;
        private Level m_levelFatal;


        private readonly static Type ThisDeclaringType = typeof(LogImpl);
    }
}
