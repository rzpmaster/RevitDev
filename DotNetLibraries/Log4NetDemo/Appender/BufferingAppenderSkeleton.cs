using System;
using System.Collections;
using Log4NetDemo.Appender.Interface.Evaluator;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;

namespace Log4NetDemo.Appender
{
    /// <summary>
    /// 具有缓冲区类型的 Appender 基类
    /// </summary>
    public abstract class BufferingAppenderSkeleton : AppenderSkeleton
    {
        protected BufferingAppenderSkeleton() : this(true)
        {
        }

        protected BufferingAppenderSkeleton(bool eventMustBeFixed) : base()
        {
            m_eventMustBeFixed = eventMustBeFixed;
        }

        #region Public Instance Properties

        /// <summary>
        /// 指示是否是 Lossy 模式
        /// </summary>
        /// <remarks>
        /// <para>如果为<value>false</value>，缓冲区满时，就会发送</para>
        /// <para>如果为<value>true</value>，只有Evaluator满足才会发送</para>
        /// </remarks>
        public bool Lossy
        {
            get { return m_lossy; }
            set { m_lossy = value; }
        }

        public int BufferSize
        {
            get { return m_bufferSize; }
            set { m_bufferSize = value; }
        }

        public ITriggeringEventEvaluator Evaluator
        {
            get { return m_evaluator; }
            set { m_evaluator = value; }
        }

        public ITriggeringEventEvaluator LossyEvaluator
        {
            get { return m_lossyEvaluator; }
            set { m_lossyEvaluator = value; }
        }

        /// <summary>
        /// 缓冲Appender 使用的  
        /// </summary>
        virtual public FixFlags Fix
        {
            get { return m_fixFlags; }
            set { m_fixFlags = value; }
        }

        #endregion

        #region Implementation of IOptionHandler

        override public void ActivateOptions()
        {
            base.ActivateOptions();

            // If the appender is in Lossy mode then we will
            // only send the buffer when the Evaluator triggers
            // therefore check we have an evaluator.
            if (m_lossy && m_evaluator == null)
            {
                ErrorHandler.Error("Appender [" + Name + "] is Lossy but has no Evaluator. The buffer will never be sent!");
            }

            if (m_bufferSize > 1)
            {
                m_cb = new CyclicBuffer(m_bufferSize);
            }
            else
            {
                m_cb = null;
            }
        }

        #endregion

        #region Override implementation of AppenderSkeleton

        override protected void OnClose()
        {
            // Flush the buffer on close
            Flush(true);
        }

        /// <summary>
        /// This method is called by the <see cref="AppenderSkeleton.DoAppend(LoggingEvent)"/> method. 
        /// </summary>
        /// <param name="loggingEvent"></param>
        override protected void Append(LoggingEvent loggingEvent)
        {
            // If the buffer size is set to 1 or less then the buffer will be
            // sent immediately because there is not enough space in the buffer
            // to buffer up more than 1 event. Therefore as a special case
            // we don't use the buffer at all.
            // 缓冲区的大小为 1 的话是不需要考虑缓存的，因为没有空间处理
            if (m_cb == null || m_bufferSize <= 1)
            {
                // Only send the event if we are in non lossy mode or the event is a triggering event
                if ((!m_lossy) ||
                    (m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent)) ||
                    (m_lossyEvaluator != null && m_lossyEvaluator.IsTriggeringEvent(loggingEvent)))
                {
                    if (m_eventMustBeFixed)
                    {
                        // Derive class expects fixed events
                        loggingEvent.Fix = this.Fix;
                    }

                    // Not buffering events, send immediately
                    SendBuffer(new LoggingEvent[] { loggingEvent });
                }
            }
            else
            {
                // Because we are caching the LoggingEvent beyond the
                // lifetime of the Append() method we must fix any
                // volatile data in the event.
                loggingEvent.Fix = this.Fix;

                // Add to the buffer, returns the event discarded from the buffer if there is no space remaining after the append
                // 先缓存起来
                LoggingEvent discardedLoggingEvent = m_cb.Append(loggingEvent);

                if (discardedLoggingEvent != null)
                {
                    // Buffer is full and has had to discard an event
                    if (!m_lossy)
                    {
                        // Not lossy, must send all events
                        SendFromBuffer(discardedLoggingEvent, m_cb);
                    }
                    else
                    {
                        // Check if the discarded event should not be logged
                        if (m_lossyEvaluator == null || !m_lossyEvaluator.IsTriggeringEvent(discardedLoggingEvent))
                        {
                            // Clear the discarded event as we should not forward it
                            discardedLoggingEvent = null;
                        }

                        // Check if the event should trigger the whole buffer to be sent
                        if (m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent))
                        {
                            SendFromBuffer(discardedLoggingEvent, m_cb);
                        }
                        else if (discardedLoggingEvent != null)
                        {
                            // Just send the discarded event
                            SendBuffer(new LoggingEvent[] { discardedLoggingEvent });
                        }
                    }
                }
                else
                {
                    // Buffer is not yet full

                    // Check if the event should trigger the whole buffer to be sent
                    if (m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent))
                    {
                        SendFromBuffer(null, m_cb);
                    }
                }
            }
        }

        #endregion

        #region Implementation of IFlushable

        public override bool Flush(int millisecondsTimeout)
        {
            Flush();
            return true;
        }

        public virtual void Flush()
        {
            Flush(false);
        }

        /// <summary>
        /// Flush the currently buffered events
        /// </summary>
        /// <param name="flushLossyBuffer">set to <c>true</c> to flush the buffer of lossy events</param>
        public virtual void Flush(bool flushLossyBuffer)
        {
            // This method will be called outside of the AppenderSkeleton DoAppend() method
            // therefore it needs to be protected by its own lock. This will block any
            // Appends while the buffer is flushed.
            lock (this)
            {
                if (m_cb != null && m_cb.Length > 0)
                {
                    if (m_lossy)
                    {
                        // If we are allowed to eagerly flush from the lossy buffer
                        if (flushLossyBuffer)
                        {
                            if (m_lossyEvaluator != null)
                            {
                                // Test the contents of the buffer against the lossy evaluator
                                LoggingEvent[] bufferedEvents = m_cb.PopAll();
                                ArrayList filteredEvents = new ArrayList(bufferedEvents.Length);

                                foreach (LoggingEvent loggingEvent in bufferedEvents)
                                {
                                    if (m_lossyEvaluator.IsTriggeringEvent(loggingEvent))
                                    {
                                        filteredEvents.Add(loggingEvent);
                                    }
                                }

                                // Send the events that meet the lossy evaluator criteria
                                if (filteredEvents.Count > 0)
                                {
                                    SendBuffer((LoggingEvent[])filteredEvents.ToArray(typeof(LoggingEvent)));
                                }
                            }
                            else
                            {
                                // No lossy evaluator, all buffered events are discarded
                                m_cb.Clear();
                            }
                        }
                    }
                    else
                    {
                        // Not lossy, send whole buffer
                        SendFromBuffer(null, m_cb);
                    }
                }
            }
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// 发送缓冲区里的日志消息给所有 Apeender
        /// </summary>
        /// <param name="firstLoggingEvent">原来在缓冲器中被挤出来的最古老的消息</param>
        /// <param name="buffer">缓冲区的日志消息</param>
        virtual protected void SendFromBuffer(LoggingEvent firstLoggingEvent, CyclicBuffer buffer)
        {
            LoggingEvent[] bufferEvents = buffer.PopAll();

            if (firstLoggingEvent == null)
            {
                SendBuffer(bufferEvents);
            }
            else if (bufferEvents.Length == 0)
            {
                SendBuffer(new LoggingEvent[] { firstLoggingEvent });
            }
            else
            {
                // Create new array with the firstLoggingEvent at the head
                LoggingEvent[] events = new LoggingEvent[bufferEvents.Length + 1];
                Array.Copy(bufferEvents, 0, events, 1, bufferEvents.Length);
                events[0] = firstLoggingEvent;

                SendBuffer(events);
            }
        }

        /// <summary>
        /// 将所有消息发送给 Apender 处理
        /// </summary>
        /// <param name="events"></param>
        abstract protected void SendBuffer(LoggingEvent[] events);

        #endregion


        private const int DEFAULT_BUFFER_SIZE = 512;

        private int m_bufferSize = DEFAULT_BUFFER_SIZE;
        private CyclicBuffer m_cb;

        /// <summary>
        /// 当缓冲区满时，是应该 overwrite，还是 flushed
        /// </summary>
        private bool m_lossy = false;

        private ITriggeringEventEvaluator m_evaluator;
        private ITriggeringEventEvaluator m_lossyEvaluator;

#pragma warning disable CS0414
        private FixFlags m_fixFlags = FixFlags.All;
#pragma warning restore CS0414

#pragma warning disable CS0169 
        private readonly bool m_eventMustBeFixed;
#pragma warning restore CS0169 
    }

    /// <summary>
    /// 环形队列
    /// </summary>
    public class CyclicBuffer
    {
        /// <summary>
        /// 内部维护的 LoggingEvent 数组
        /// </summary>
        private LoggingEvent[] m_events;
        private int m_maxSize;

        public CyclicBuffer(int maxSize)
        {
            if (maxSize < 1)
            {
                throw SystemInfo.CreateArgumentOutOfRangeException("maxSize", (object)maxSize, "Parameter: maxSize, Value: [" + maxSize + "] out of range. Non zero positive integer required");
            }

            m_maxSize = maxSize;
            m_events = new LoggingEvent[maxSize];

            m_first = 0;
            m_last = 0;
            m_numElems = 0;
        }

        public LoggingEvent Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            lock (this)
            {
                // save the discarded event
                LoggingEvent discardedLoggingEvent = m_events[m_last];

                // overwrite the last event position
                m_events[m_last] = loggingEvent;
                if (++m_last == m_maxSize)
                {
                    m_last = 0;
                }

                if (m_numElems < m_maxSize)
                {
                    m_numElems++;
                }
                else if (++m_first == m_maxSize)
                {
                    m_first = 0;
                }

                if (m_numElems < m_maxSize)
                {
                    // Space remaining
                    return null;
                }
                else
                {
                    // Buffer is full and discarding an event
                    // 缓存区已经满了，返回丢弃的日志事件信息
                    return discardedLoggingEvent;
                }
            }
        }

        public LoggingEvent PopOldest()
        {
            lock (this)
            {
                LoggingEvent ret = null;
                if (m_numElems > 0)
                {
                    m_numElems--;
                    ret = m_events[m_first];
                    m_events[m_first] = null;
                    if (++m_first == m_maxSize)
                    {
                        m_first = 0;
                    }
                }
                return ret;
            }
        }

        public LoggingEvent[] PopAll()
        {
            lock (this)
            {
                LoggingEvent[] ret = new LoggingEvent[m_numElems];

                if (m_numElems > 0)
                {
                    if (m_first < m_last)
                    {
                        Array.Copy(m_events, m_first, ret, 0, m_numElems);
                    }
                    else
                    {
                        Array.Copy(m_events, m_first, ret, 0, m_maxSize - m_first);
                        Array.Copy(m_events, 0, ret, m_maxSize - m_first, m_last);
                    }
                }

                Clear();

                return ret;
            }
        }

        public void Clear()
        {
            lock (this)
            {
                // Set all the elements to null
                Array.Clear(m_events, 0, m_events.Length);

                m_first = 0;
                m_last = 0;
                m_numElems = 0;
            }
        }

        public LoggingEvent this[int i]
        {
            get
            {
                lock (this)
                {
                    if (i < 0 || i >= m_numElems)
                    {
                        return null;
                    }

                    return m_events[(m_first + i) % m_maxSize];
                }
            }
        }

        public int MaxSize
        {
            get
            {
                lock (this)
                {
                    return m_maxSize;
                }
            }
        }

        public int Length
        {
            get
            {
                lock (this)
                {
                    return m_numElems;
                }
            }
        }

        private int m_first;
        private int m_last;
        private int m_numElems;
    }

}
