using System;
using Log4NetDemo.Appender.AppenderAttach;
using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender
{
    /// <summary>
    /// 缓冲器，然后将消息分发给其他 Appender
    /// </summary>
    /// <remarks>
    /// <para>日志消息会现在这个类中缓存，等达到某个条件，再被分发到其他Appender中</para>
    /// <para>可以为在链表结构中不同位置的同一Appender指定不同的筛选器</para>
    /// </remarks>
    public class BufferingForwardingAppender : BufferingAppenderSkeleton, IAppenderAttachable
    {
        public BufferingForwardingAppender()
        {
        }

        #region Override implementation of BufferingAppenderSkeleton

        override protected void SendBuffer(LoggingEvent[] events)
        {
            // Pass the logging event on to the attached appenders
            if (m_appenderAttachedImpl != null)
            {
                m_appenderAttachedImpl.AppendLoopOnAppenders(events);
            }
        }

        override protected void OnClose()
        {
            // Remove all the attached appenders
            lock (this)
            {
                // Delegate to base, which will flush buffers
                base.OnClose();

                if (m_appenderAttachedImpl != null)
                {
                    m_appenderAttachedImpl.RemoveAllAppenders();
                }
            }
        }

        #endregion

        #region Implementation of IAppenderAttachable

        virtual public void AddAppender(IAppender newAppender)
        {
            if (newAppender == null)
            {
                throw new ArgumentNullException("newAppender");
            }
            lock (this)
            {
                if (m_appenderAttachedImpl == null)
                {
                    m_appenderAttachedImpl = new AppenderAttachedImpl();
                }
                m_appenderAttachedImpl.AddAppender(newAppender);
            }
        }

        virtual public AppenderCollection Appenders
        {
            get
            {
                lock (this)
                {
                    if (m_appenderAttachedImpl == null)
                    {
                        return AppenderCollection.EmptyCollection;
                    }
                    else
                    {
                        return m_appenderAttachedImpl.Appenders;
                    }
                }
            }
        }
        virtual public IAppender GetAppender(string name)
        {
            lock (this)
            {
                if (m_appenderAttachedImpl == null || name == null)
                {
                    return null;
                }

                return m_appenderAttachedImpl.GetAppender(name);
            }
        }

        virtual public void RemoveAllAppenders()
        {
            lock (this)
            {
                if (m_appenderAttachedImpl != null)
                {
                    m_appenderAttachedImpl.RemoveAllAppenders();
                    m_appenderAttachedImpl = null;
                }
            }
        }

        virtual public IAppender RemoveAppender(IAppender appender)
        {
            lock (this)
            {
                if (appender != null && m_appenderAttachedImpl != null)
                {
                    return m_appenderAttachedImpl.RemoveAppender(appender);
                }
            }
            return null;
        }

        virtual public IAppender RemoveAppender(string name)
        {
            lock (this)
            {
                if (name != null && m_appenderAttachedImpl != null)
                {
                    return m_appenderAttachedImpl.RemoveAppender(name);
                }
            }
            return null;
        }


        #endregion

        private AppenderAttachedImpl m_appenderAttachedImpl;
    }
}
