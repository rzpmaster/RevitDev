using Log4NetDemo.Core.Interface;
using System;
using System.Collections;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// Delegate type used for LogicalThreadContextStack's callbacks.
    /// </summary>
    public delegate void TwoArgAction(string t1, LogicalThreadContextStack t2);

    /// <summary>
    /// 逻辑线程上下文栈
    /// </summary>
    public sealed class LogicalThreadContextStack : IFixingRequired
    {
        #region Private Instance Fields

        /// <summary>
        /// The name of this <see cref="Log4NetDemo.Context.LogicalThreadContextStack"/> within the
        /// <see cref="Log4NetDemo.Context.LogicalThreadContextProperties"/>.
        /// </summary>
        private string m_propertyKey;

        /// <summary>
        /// The callback used to let the <see cref="Log4NetDemo.Context.LogicalThreadContextStacks"/> register a
        /// new instance of a <see cref="Log4NetDemo.Context.LogicalThreadContextStack"/>.
        /// </summary>
        private TwoArgAction m_registerNew;

        #endregion Private Instance Fields

        internal LogicalThreadContextStack(string propertyKey, TwoArgAction registerNew)
        {
            m_propertyKey = propertyKey;
            m_registerNew = registerNew;
        }

        /// <summary>
        /// 内部维护的栈，栈里放的是 StackFrame
        /// </summary>
        private Stack m_stack = new Stack();
        /// <summary>
        /// Gets and sets the internal stack used by this <see cref="LogicalThreadContextStack"/>
        /// </summary>
        /// <value>The internal storage stack</value>
        /// <remarks>
        /// <para>
        /// This property is provided only to support backward compatability 
        /// of the <see cref="NDC"/>. Tytpically the internal stack should not
        /// be modified.
        /// </para>
        /// <para>
        /// 提供此属性只是为了支持向后兼容<see cref="NDC"/>。通常内部堆栈不应该被修改。
        /// </para>
        /// </remarks>
        internal Stack InternalStack
        {
            get { return m_stack; }
            set { m_stack = value; }
        }

        #region Public Properties

        /// <summary>
        /// The number of messages in the stack
        /// </summary>
        /// <value>
        /// The current number of messages in the stack
        /// </value>
        /// <remarks>
        /// <para>
        /// The current number of messages in the stack. That is
        /// the number of times <see cref="Push"/> has been called
        /// minus the number of times <see cref="Pop"/> has been called.
        /// </para>
        /// </remarks>
        public int Count
        {
            get { return m_stack.Count; }
        }

        #endregion // Public Properties

        #region Public Methods

        /// <summary>
        /// Clears all the contextual information held in this stack.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clears all the contextual information held in this stack.
        /// Only call this if you think that this thread is being reused after
        /// a previous call execution which may not have completed correctly.
        /// You do not need to use this method if you always guarantee to call
        /// the <see cref="IDisposable.Dispose"/> method of the <see cref="IDisposable"/>
        /// returned from <see cref="Push"/> even in exceptional circumstances,
        /// for example by using the <c>using(log4net.LogicalThreadContext.Stacks["NDC"].Push("Stack_Message"))</c> 
        /// syntax.
        /// </para>
        /// </remarks>
        public void Clear()
        {
            m_registerNew(m_propertyKey, new LogicalThreadContextStack(m_propertyKey, m_registerNew));
        }

        /// <summary>
        /// Removes the top context from this stack.
        /// </summary>
        /// <returns>The message in the context that was removed from the top of this stack.</returns>
        /// <remarks>
        /// <para>
        /// Remove the top context from this stack, and return
        /// it to the caller. If this stack is empty then an
        /// empty string (not <see langword="null"/>) is returned.
        /// </para>
        /// </remarks>
        public string Pop()
        {
            // copy current stack
            Stack stack = new Stack(new Stack(m_stack));
            string result = "";
            if (stack.Count > 0)
            {
                result = ((StackFrame)(stack.Pop())).Message;
            }
            LogicalThreadContextStack ltcs = new LogicalThreadContextStack(m_propertyKey, m_registerNew);
            ltcs.m_stack = stack;
            m_registerNew(m_propertyKey, ltcs);
            return result;
        }

        /// <summary>
        /// Pushes a new context message into this stack.
        /// </summary>
        /// <param name="message">The new context message.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> that can be used to clean up the context stack.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Pushes a new context onto this stack. An <see cref="IDisposable"/>
        /// is returned that can be used to clean up this stack. This
        /// can be easily combined with the <c>using</c> keyword to scope the
        /// context.
        /// </para>
        /// </remarks>
        /// <example>Simple example of using the <c>Push</c> method with the <c>using</c> keyword.
        /// <code lang="C#">
        /// using(log4net.LogicalThreadContext.Stacks["NDC"].Push("Stack_Message"))
        /// {
        ///		log.Warn("This should have an ThreadContext Stack message");
        ///	}
        /// </code>
        /// </example>
        public IDisposable Push(string message)
        {
            // do modifications on a copy
            Stack stack = new Stack(new Stack(m_stack));
            stack.Push(new StackFrame(message, (stack.Count > 0) ? (StackFrame)stack.Peek() : null));

            LogicalThreadContextStack contextStack = new LogicalThreadContextStack(m_propertyKey, m_registerNew);
            contextStack.m_stack = stack;
            m_registerNew(m_propertyKey, contextStack);
            return new AutoPopStackFrame(contextStack, stack.Count - 1);
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Gets the current context information for this stack.
        /// </summary>
        /// <returns>The current context information.</returns>
        internal string GetFullMessage()
        {
            Stack stack = m_stack;
            if (stack.Count > 0)
            {
                return ((StackFrame)(stack.Peek())).FullMessage;
            }
            return null;
        }

        #endregion Internal Methods

        /// <summary>
        /// Gets the current context information for this stack.
        /// </summary>
        /// <returns>Gets the current context information</returns>
        /// <remarks>
        /// <para>
        /// Gets the current context information for this stack.
        /// </para>
        /// </remarks>
        public override string ToString()
        {
            return GetFullMessage();
        }

        #region Implementation of IFixingRequired

        object IFixingRequired.GetFixedObject()
        {
            return GetFullMessage();
        }

        #endregion

        /// <summary>
        /// 表示压入栈的对象，内部类（链式结构）
        /// </summary>
        private sealed class StackFrame
        {
            #region Private Instance Fields

            private readonly string m_message;
            private readonly StackFrame m_parent;
            private string m_fullMessage = null;

            #endregion

            #region Internal Instance Constructors

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="message">The message for this context.</param>
            /// <param name="parent">The parent context in the chain.</param>
            /// <remarks>
            /// <para>
            /// Initializes a new instance of the <see cref="StackFrame" /> class
            /// with the specified message and parent context.
            /// </para>
            /// </remarks>
            internal StackFrame(string message, StackFrame parent)
            {
                m_message = message;
                m_parent = parent;

                if (parent == null)
                {
                    m_fullMessage = message;
                }
            }

            #endregion Internal Instance Constructors

            #region Internal Instance Properties

            /// <summary>
            /// Get the message.
            /// </summary>
            /// <value>The message.</value>
            /// <remarks>
            /// <para>
            /// Get the message.
            /// </para>
            /// </remarks>
            internal string Message
            {
                get { return m_message; }
            }

            /// <summary>
            /// Gets the full text of the context down to the root level.
            /// </summary>
            /// <value>
            /// The full text of the context down to the root level.
            /// </value>
            /// <remarks>
            /// <para>
            /// Gets the full text of the context down to the root level.
            /// </para>
            /// </remarks>
            internal string FullMessage
            {
                get
                {
                    if (m_fullMessage == null && m_parent != null)
                    {
                        m_fullMessage = string.Concat(m_parent.FullMessage, " ", m_message);
                    }
                    return m_fullMessage;
                }
            }

            #endregion Internal Instance Properties
        }

        /// <summary>
        /// Struct returned from the <see cref="LogicalThreadContextStack.Push"/> method.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This struct implements the <see cref="IDisposable"/> and is designed to be used
        /// with the <see langword="using"/> pattern to remove the stack frame at the end of the scope.
        /// </para>
        /// </remarks>
        private struct AutoPopStackFrame : IDisposable
        {
            #region Private Instance Fields

            /// <summary>
            /// The depth to trim the stack to when this instance is disposed
            /// </summary>
            private int m_frameDepth;

            /// <summary>
            /// The outer LogicalThreadContextStack.
            /// </summary>
            private LogicalThreadContextStack m_logicalThreadContextStack;

            #endregion Private Instance Fields

            #region Internal Instance Constructors

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="logicalThreadContextStack">The internal stack used by the ThreadContextStack.</param>
            /// <param name="frameDepth">The depth to return the stack to when this object is disposed.</param>
            /// <remarks>
            /// <para>
            /// Initializes a new instance of the <see cref="AutoPopStackFrame" /> class with
            /// the specified stack and return depth.
            /// </para>
            /// </remarks>
            internal AutoPopStackFrame(LogicalThreadContextStack logicalThreadContextStack, int frameDepth)
            {
                m_frameDepth = frameDepth;
                m_logicalThreadContextStack = logicalThreadContextStack;
            }

            #endregion Internal Instance Constructors

            #region Implementation of IDisposable

            /// <summary>
            /// Returns the stack to the correct depth.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Returns the stack to the correct depth.
            /// </para>
            /// </remarks>
            public void Dispose()
            {
                if (m_frameDepth >= 0 && m_logicalThreadContextStack.m_stack != null)
                {
                    Stack stack = new Stack(new Stack(m_logicalThreadContextStack.m_stack));
                    while (stack.Count > m_frameDepth)
                    {
                        stack.Pop();
                    }

                    LogicalThreadContextStack ltcs = new LogicalThreadContextStack(m_logicalThreadContextStack.m_propertyKey, m_logicalThreadContextStack.m_registerNew);
                    ltcs.m_stack = stack;

                    // 每次 Pop() 后，用新的栈，将原来的替换掉
                    m_logicalThreadContextStack.m_registerNew(m_logicalThreadContextStack.m_propertyKey,
                        ltcs);
                }
            }

            #endregion Implementation of IDisposable
        }

    }
}
