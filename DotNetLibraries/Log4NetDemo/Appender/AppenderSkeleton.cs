using Log4NetDemo.Appender.ErrorHandler;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Filter;
using Log4NetDemo.Layout;
using Log4NetDemo.Util;
using System;
using System.Collections;
using System.IO;

namespace Log4NetDemo.Appender
{
    /// <summary>
    /// Appender 抽象基类
    /// </summary>
    public abstract class AppenderSkeleton : IBulkAppender, IFlushable, IOptionHandler, IAppender
    {
        protected AppenderSkeleton()
        {
            m_errorHandler = new OnlyOnceErrorHandler(this.GetType().Name);
        }

        ~AppenderSkeleton()
        {
            // An appender might be closed then garbage collected. 
            // There is no point in closing twice.
            if (!m_closed)
            {
                LogLog.Debug(declaringType, "Finalizing appender named [" + m_name + "].");
                Close();
            }
        }

        #region Public Instance Properties

        public Level Threshold
        {
            get { return m_threshold; }
            set { m_threshold = value; }
        }

        virtual public IErrorHandler ErrorHandler
        {
            get { return this.m_errorHandler; }
            set
            {
                lock (this)
                {
                    if (value == null)
                    {
                        // We do not throw exception here since the cause is probably a
                        // bad config file.
                        LogLog.Warn(declaringType, "You have tried to set a null error-handler.");
                    }
                    else
                    {
                        m_errorHandler = value;
                    }
                }
            }
        }

        virtual public IFilter FilterHead
        {
            get { return m_headFilter; }
        }

        virtual public ILayout Layout
        {
            get { return m_layout; }
            set { m_layout = value; }
        }

        #endregion

        #region Implementation of IAppender

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public void Close()
        {
            // This lock prevents the appender being closed while it is still appending
            lock (this)
            {
                if (!m_closed)
                {
                    OnClose();
                    m_closed = true;
                }
            }
        }

        /// <summary>
		/// Performs threshold checks and invokes filters before 
		/// delegating actual logging to the subclasses specific 
		/// <see cref="Append(LoggingEvent)"/> method.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// This method cannot be overridden by derived classes. A
		/// derived class should override the <see cref="Append(LoggingEvent)"/> method
		/// which is called by this method.
		/// </para>
		/// <para>
		/// The implementation of this method is as follows:
		/// </para>
		/// <para>
		/// <list type="bullet">
		///		<item>
		///			<description>
		///			Checks that the severity of the <paramref name="loggingEvent"/>
		///			is greater than or equal to the <see cref="Threshold"/> of this
		///			appender.
        ///			</description>
		///		</item>
		///		<item>
		///			<description>
		///			Checks that the <see cref="IFilter"/> chain accepts the 
		///			<paramref name="loggingEvent"/>.
		///			</description>
		///		</item>
		///		<item>
		///			<description>
		///			Calls <see cref="PreAppendCheck()"/> and checks that 
		///			it returns <c>true</c>.
        ///			</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// If all of the above steps succeed then the <paramref name="loggingEvent"/>
		/// will be passed to the abstract <see cref="Append(LoggingEvent)"/> method.
		/// </para>
		/// </remarks>
		public void DoAppend(LoggingEvent loggingEvent)
        {
            // This lock is absolutely critical for correct formatting
            // of the message in a multi-threaded environment.  Without
            // this, the message may be broken up into elements from
            // multiple thread contexts (like get the wrong thread ID).

            lock (this)
            {
                if (m_closed)
                {
                    ErrorHandler.Error("Attempted to append to closed appender named [" + m_name + "].");
                    return;
                }

                // prevent re-entry
                if (m_recursiveGuard)
                {
                    return;
                }

                try
                {
                    m_recursiveGuard = true;

                    if (FilterEvent(loggingEvent) && PreAppendCheck())
                    {
                        this.Append(loggingEvent);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.Error("Failed in DoAppend", ex);
                }
                finally
                {
                    m_recursiveGuard = false;
                }
            }
        }

        #endregion

        #region Implementation of IBulkAppender

        /// <summary>
        /// Performs threshold checks and invokes filters before 
        /// delegating actual logging to the subclasses specific 
        /// <see cref="Append(LoggingEvent[])"/> method.
        /// </summary>
        /// <param name="loggingEvents">The array of events to log.</param>
        /// <remarks>
        /// <para>
        /// This method cannot be overridden by derived classes. A
        /// derived class should override the <see cref="Append(LoggingEvent[])"/> method
        /// which is called by this method.
        /// </para>
        /// <para>
        /// The implementation of this method is as follows:
        /// </para>
        /// <para>
        /// <list type="bullet">
        ///		<item>
        ///			<description>
        ///			Checks that the severity of the <paramref name="loggingEvents"/>
        ///			is greater than or equal to the <see cref="Threshold"/> of this
        ///			appender.</description>
        ///		</item>
        ///		<item>
        ///			<description>
        ///			Checks that the <see cref="IFilter"/> chain accepts the 
        ///			<paramref name="loggingEvents"/>.
        ///			</description>
        ///		</item>
        ///		<item>
        ///			<description>
        ///			Calls <see cref="PreAppendCheck()"/> and checks that 
        ///			it returns <c>true</c>.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// <para>
        /// If all of the above steps succeed then the <paramref name="loggingEvents"/>
        /// will be passed to the <see cref="Append(LoggingEvent[])"/> method.
        /// </para>
        /// </remarks>
        public void DoAppend(LoggingEvent[] loggingEvents)
        {
            // This lock is absolutely critical for correct formatting
            // of the message in a multi-threaded environment.  Without
            // this, the message may be broken up into elements from
            // multiple thread contexts (like get the wrong thread ID).

            lock (this)
            {
                if (m_closed)
                {
                    ErrorHandler.Error("Attempted to append to closed appender named [" + m_name + "].");
                    return;
                }

                // prevent re-entry
                if (m_recursiveGuard)
                {
                    return;
                }

                try
                {
                    m_recursiveGuard = true;

                    ArrayList filteredEvents = new ArrayList(loggingEvents.Length);

                    foreach (LoggingEvent loggingEvent in loggingEvents)
                    {
                        if (FilterEvent(loggingEvent))
                        {
                            filteredEvents.Add(loggingEvent);
                        }
                    }

                    if (filteredEvents.Count > 0 && PreAppendCheck())
                    {
                        this.Append((LoggingEvent[])filteredEvents.ToArray(typeof(LoggingEvent)));
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.Error("Failed in Bulk DoAppend", ex);
                }
                finally
                {
                    m_recursiveGuard = false;
                }
            }
        }

        #endregion Implementation of IBulkAppender

        #region Implementation of IFlushable

        /// <summary>
        /// Flushes any buffered log data.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        /// <remarks>
        /// This implementation doesn't flush anything and always returns true
        /// </remarks>
        public virtual bool Flush(int millisecondsTimeout)
        {
            return true;
        }

        #endregion

        #region Implementation of IOptionHandler

        virtual public void ActivateOptions()
        {
        }

        #endregion Implementation of IOptionHandler

        #region Public Instance Methods

        virtual public void AddFilter(IFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter param must not be null");
            }

            if (m_headFilter == null)
            {
                m_headFilter = m_tailFilter = filter;
            }
            else
            {
                m_tailFilter.Next = filter;
                m_tailFilter = filter;
            }
        }

        virtual public void ClearFilters()
        {
            m_headFilter = m_tailFilter = null;
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Test if the logging event should we output by this appender
        /// </summary>
        /// <param name="loggingEvent">the event to test</param>
        /// <returns><c>true</c> if the event should be output, <c>false</c> if the event should be ignored</returns>
        /// <remarks>
        /// <para>
        /// This method checks the logging event against the threshold level set
        /// on this appender and also against the filters specified on this
        /// appender.
        /// </para>
        /// <para>
        /// The implementation of this method is as follows:
        /// </para>
        /// <para>
        /// <list type="bullet">
        ///		<item>
        ///			<description>
        ///			Checks that the severity of the <paramref name="loggingEvent"/>
        ///			is greater than or equal to the <see cref="Threshold"/> of this
        ///			appender.</description>
        ///		</item>
        ///		<item>
        ///			<description>
        ///			Checks that the <see cref="IFilter"/> chain accepts the 
        ///			<paramref name="loggingEvent"/>.
        ///			</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        virtual protected bool FilterEvent(LoggingEvent loggingEvent)
        {
            if (!IsAsSevereAsThreshold(loggingEvent.Level))
            {
                return false;
            }

            IFilter f = this.FilterHead;

            while (f != null)
            {
                switch (f.Decide(loggingEvent))
                {
                    case FilterDecision.Deny:
                        return false;   // Return without appending

                    case FilterDecision.Accept:
                        f = null;       // Break out of the loop
                        break;

                    case FilterDecision.Neutral:
                        f = f.Next;     // Move to next filter
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the message level is below this appender's threshold.
        /// </summary>
        /// <param name="level"><see cref="Level"/> to test against.</param>
        /// <remarks>
        /// <para>
        /// If there is no threshold set, then the return value is always <c>true</c>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// <c>true</c> if the <paramref name="level"/> meets the <see cref="Threshold"/> 
        /// requirements of this appender.
        /// </returns>
        virtual protected bool IsAsSevereAsThreshold(Level level)
        {
            return ((m_threshold == null) || level >= m_threshold);
        }

        /// <summary>
        /// Is called when the appender is closed. Derived classes should override 
        /// this method if resources need to be released.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Releases any resources allocated within the appender such as file handles, 
        /// network connections, etc.
        /// </para>
        /// <para>
        /// It is a programming error to append to a closed appender.
        /// </para>
        /// </remarks>
        virtual protected void OnClose()
        {
            // Do nothing by default
            // 默认什么都不做，派生类应该重写此方法，释放非托管资源
        }

        /// <summary>
        /// Subclasses of <see cref="AppenderSkeleton"/> should implement this method 
        /// to perform actual logging.
        /// </summary>
        /// <param name="loggingEvent">The event to append.</param>
        /// <remarks>
        /// <para>
        /// A subclass must implement this method to perform
        /// logging of the <paramref name="loggingEvent"/>.
        /// </para>
        /// <para>This method will be called by <see cref="DoAppend(LoggingEvent)"/>
        /// if all the conditions listed for that method are met.
        /// </para>
        /// <para>
        /// To restrict the logging of events in the appender
        /// override the <see cref="PreAppendCheck()"/> method.
        /// </para>
        /// </remarks>
        abstract protected void Append(LoggingEvent loggingEvent);

        /// <summary>
        /// Append a bulk array of logging events.
        /// </summary>
        /// <param name="loggingEvents">the array of logging events</param>
        /// <remarks>
        /// <para>
        /// This base class implementation calls the <see cref="Append(LoggingEvent)"/>
        /// method for each element in the bulk array.
        /// </para>
        /// <para>
        /// A sub class that can better process a bulk array of events should
        /// override this method in addition to <see cref="Append(LoggingEvent)"/>.
        /// </para>
        /// </remarks>
        virtual protected void Append(LoggingEvent[] loggingEvents)
        {
            foreach (LoggingEvent loggingEvent in loggingEvents)
            {
                Append(loggingEvent);
            }
        }

        /// <summary>
        /// Called before <see cref="Append(LoggingEvent)"/> as a precondition.(先决条件)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called by <see cref="DoAppend(LoggingEvent)"/>
        /// before the call to the abstract <see cref="Append(LoggingEvent)"/> method.
        /// </para>
        /// <para>
        /// This method can be overridden in a subclass to extend the checks 
        /// made before the event is passed to the <see cref="Append(LoggingEvent)"/> method.
        /// </para>
        /// <para>
        /// A subclass should ensure that they delegate this call to
        /// this base class if it is overridden.
        /// </para>
        /// </remarks>
        /// <returns><c>true</c> if the call to <see cref="Append(LoggingEvent)"/> should proceed.</returns>
        virtual protected bool PreAppendCheck()
        {
            if ((m_layout == null) && RequiresLayout)
            {
                ErrorHandler.Error("AppenderSkeleton: No layout set for the appender named [" + m_name + "].");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Renders the <see cref="LoggingEvent"/> to a string.
        /// </summary>
        /// <param name="loggingEvent">The event to render.</param>
        /// <returns>The event rendered as a string.</returns>
        /// <remarks>
        /// <para>
        /// Helper method to render a <see cref="LoggingEvent"/> to 
        /// a string. This appender must have a <see cref="Layout"/>
        /// set to render the <paramref name="loggingEvent"/> to 
        /// a string.
        /// </para>
        /// <para>If there is exception data in the logging event and 
        /// the layout does not process the exception, this method 
        /// will append the exception text to the rendered string.
        /// </para>
        /// <para>
        /// Where possible use the alternative version of this method
        /// <see cref="RenderLoggingEvent(TextWriter,LoggingEvent)"/>.
        /// That method streams the rendering onto an existing Writer
        /// which can give better performance if the caller already has
        /// a <see cref="TextWriter"/> open and ready for writing.
        /// </para>
        /// </remarks>
        protected string RenderLoggingEvent(LoggingEvent loggingEvent)
        {
            // Create the render writer on first use
            if (m_renderWriter == null)
            {
                m_renderWriter = new ReusableStringWriter(System.Globalization.CultureInfo.InvariantCulture);
            }

            lock (m_renderWriter)
            {
                // Reset the writer so we can reuse it
                m_renderWriter.Reset(c_renderBufferMaxCapacity, c_renderBufferSize);

                RenderLoggingEvent(m_renderWriter, loggingEvent);
                return m_renderWriter.ToString();
            }
        }

        /// <summary>
        /// Renders the <see cref="LoggingEvent"/> to a string.
        /// </summary>
        /// <param name="loggingEvent">The event to render.</param>
        /// <param name="writer">The TextWriter to write the formatted event to</param>
        /// <remarks>
        /// <para>
        /// Helper method to render a <see cref="LoggingEvent"/> to 
        /// a string. This appender must have a <see cref="Layout"/>
        /// set to render the <paramref name="loggingEvent"/> to 
        /// a string.
        /// </para>
        /// <para>If there is exception data in the logging event and 
        /// the layout does not process the exception, this method 
        /// will append the exception text to the rendered string.
        /// </para>
        /// <para>
        /// Use this method in preference to <see cref="RenderLoggingEvent(LoggingEvent)"/>
        /// where possible. If, however, the caller needs to render the event
        /// to a string then <see cref="RenderLoggingEvent(LoggingEvent)"/> does
        /// provide an efficient mechanism for doing so.
        /// </para>
        /// </remarks>
        protected void RenderLoggingEvent(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (m_layout == null)
            {
                throw new InvalidOperationException("A layout must be set");
            }

            if (m_layout.IgnoresException)
            {
                string exceptionStr = loggingEvent.GetExceptionString();
                if (exceptionStr != null && exceptionStr.Length > 0)
                {
                    // render the event and the exception
                    m_layout.Format(writer, loggingEvent);
                    writer.WriteLine(exceptionStr);
                }
                else
                {
                    // there is no exception to render
                    m_layout.Format(writer, loggingEvent);
                }
            }
            else
            {
                // The layout will render the exception
                m_layout.Format(writer, loggingEvent);
            }
        }

        /// <summary>
        /// Tests if this appender requires a <see cref="Layout"/> to be set.
        /// 标志此 Appender 是否需要设置 <see cref="Layout"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// In the rather exceptional case, where the appender 
        /// implementation admits a layout but can also work without it, 
        /// then the appender should return <c>true</c>.
        /// </para>
        /// <para>
        /// This default implementation always returns <c>false</c>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// <c>true</c> if the appender requires a layout object, otherwise <c>false</c>.
        /// </returns>
        virtual protected bool RequiresLayout
        {
            get { return false; }
        }

        #endregion

        #region Private Instance Fields

        private ILayout m_layout;
        private string m_name;
        private Level m_threshold;

        private IErrorHandler m_errorHandler;

        /// <summary>
        /// 过滤器链中的第一个
        /// </summary>
        private IFilter m_headFilter;
        /// <summary>
        /// 过滤器链中的最后一个
        /// </summary>
        private IFilter m_tailFilter;

        /// <summary>
        /// Appender 关闭标志
        /// </summary>
        private bool m_closed = false;

        /// <summary>
        /// 递归保护，这个字段防止appender重复调用它自己的DoAppend方法
        /// </summary>
        private bool m_recursiveGuard = false;

        private ReusableStringWriter m_renderWriter = null;

        #endregion Private Instance Fields

        #region Constants

        /// <summary>
        /// Initial buffer size
        /// </summary>
        private const int c_renderBufferSize = 256;

        /// <summary>
        /// Maximum buffer size before it is recycled
        /// </summary>
        private const int c_renderBufferMaxCapacity = 1024;

        #endregion

        private readonly static Type declaringType = typeof(AppenderSkeleton);
    }
}
