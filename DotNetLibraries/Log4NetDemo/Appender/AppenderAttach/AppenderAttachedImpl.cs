using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Appender.AppenderAttach
{
    public class AppenderAttachedImpl : IAppenderAttachable
    {
        public AppenderAttachedImpl()
        {

        }

        #region Public Instance Methods

        /// <summary>
        /// 循环所有 Appender 记录日志
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns>返回调用的 Appender 的个数 </returns>
        public int AppendLoopOnAppenders(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            // m_appenderList is null when empty
            if (m_appenderList == null)
            {
                return 0;
            }

            if (m_appenderArray == null)
            {
                m_appenderArray = m_appenderList.ToArray();
            }

            foreach (IAppender appender in m_appenderArray)
            {
                try
                {
                    appender.DoAppend(loggingEvent);
                }
                catch (Exception ex)
                {
                    LogLog.Error(declaringType, "Failed to append to appender [" + appender.Name + "]", ex);
                }
            }
            return m_appenderList.Count;
        }

        public int AppendLoopOnAppenders(LoggingEvent[] loggingEvents)
        {
            if (loggingEvents == null)
            {
                throw new ArgumentNullException("loggingEvents");
            }
            if (loggingEvents.Length == 0)
            {
                throw new ArgumentException("loggingEvents array must not be empty", "loggingEvents");
            }
            if (loggingEvents.Length == 1)
            {
                // Fall back to single event path
                return AppendLoopOnAppenders(loggingEvents[0]);
            }

            // m_appenderList is null when empty
            if (m_appenderList == null)
            {
                return 0;
            }

            if (m_appenderArray == null)
            {
                m_appenderArray = m_appenderList.ToArray();
            }

            foreach (IAppender appender in m_appenderArray)
            {
                try
                {
                    CallAppend(appender, loggingEvents);
                }
                catch (Exception ex)
                {
                    LogLog.Error(declaringType, "Failed to append to appender [" + appender.Name + "]", ex);
                }
            }
            return m_appenderList.Count;
        }

        /// <summary>
        /// 调用 Appender 中的方法记录日志
        /// </summary>
        /// <param name="appender"></param>
        /// <param name="loggingEvents"></param>
        /// <remarks>
		/// <para>
		/// If the <paramref name="appender" /> supports the <see cref="IBulkAppender"/>
		/// interface then the <paramref name="loggingEvents" /> will be passed 
		/// through using that interface. Otherwise the <see cref="LoggingEvent"/>
		/// objects in the array will be passed one at a time.
		/// </para>
		/// </remarks>
        private static void CallAppend(IAppender appender, LoggingEvent[] loggingEvents)
        {
            IBulkAppender bulkAppender = appender as IBulkAppender;
            if (bulkAppender != null)
            {
                bulkAppender.DoAppend(loggingEvents);
            }
            else
            {
                foreach (LoggingEvent loggingEvent in loggingEvents)
                {
                    appender.DoAppend(loggingEvent);
                }
            }
        }

        #endregion

        #region Implementation of IAppenderAttachable

        public void AddAppender(IAppender newAppender)
        {
            // Null values for newAppender parameter are strictly forbidden.
            if (newAppender == null)
            {
                throw new ArgumentNullException("newAppender");
            }

            m_appenderArray = null;
            if (m_appenderList == null)
            {
                m_appenderList = new AppenderCollection(1);
            }
            if (!m_appenderList.Contains(newAppender))
            {
                m_appenderList.Add(newAppender);
            }
        }

        public AppenderCollection Appenders
        {
            get
            {
                if (m_appenderList == null)
                {
                    // We must always return a valid collection
                    return AppenderCollection.EmptyCollection;
                }
                else
                {
                    return AppenderCollection.ReadOnly(m_appenderList);
                }
            }
        }

        public IAppender GetAppender(string name)
        {
            if (m_appenderList != null && name != null)
            {
                foreach (IAppender appender in m_appenderList)
                {
                    if (name == appender.Name)
                    {
                        return appender;
                    }
                }
            }
            return null;
        }

        public void RemoveAllAppenders()
        {
            if (m_appenderList != null)
            {
                foreach (IAppender appender in m_appenderList)
                {
                    try
                    {
                        appender.Close();
                    }
                    catch (Exception ex)
                    {
                        LogLog.Error(declaringType, "Failed to Close appender [" + appender.Name + "]", ex);
                    }
                }
                m_appenderList = null;
                m_appenderArray = null;
            }
        }

        public IAppender RemoveAppender(IAppender appender)
        {
            if (appender != null && m_appenderList != null)
            {
                m_appenderList.Remove(appender);
                if (m_appenderList.Count == 0)
                {
                    m_appenderList = null;
                }
                m_appenderArray = null;
            }
            return appender;
        }

        public IAppender RemoveAppender(string name)
        {
            return RemoveAppender(GetAppender(name));
        }

        #endregion

        private AppenderCollection m_appenderList;
        private IAppender[] m_appenderArray;

        private readonly static Type declaringType = typeof(AppenderAttachedImpl);
    }
}
