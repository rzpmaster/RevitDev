using Log4NetDemo.Core.Interface;
using System;
using System.Collections;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 线程上下文栈
    /// </summary>
    public sealed class ThreadContextStack : IFixingRequired
    {
        internal ThreadContextStack() { }

        /// <summary>
        /// 内部维护的栈，栈里放的是 StackFrame
        /// </summary>
        /// <remarks>
        /// Stack<StackFrame> m_stack = new Stack<StackFrame>();
        /// </remarks>
        private Stack m_stack = new Stack();
        /// <summary>
        /// Gets and sets the internal stack used by this <see cref="ThreadContextStack"/>
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
        /// Only call this if you think that this tread is being reused after
        /// a previous call execution which may not have completed correctly.
        /// You do not need to use this method if you always guarantee to call
        /// the <see cref="IDisposable.Dispose"/> method of the <see cref="IDisposable"/>
        /// returned from <see cref="Push"/> even in exceptional circumstances,
        /// for example by using the <c>using(log4net.ThreadContext.Stacks["NDC"].Push("Stack_Message"))</c> 
        /// syntax.
        /// </para>
        /// </remarks>
        public void Clear()
        {
            m_stack.Clear();
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
            Stack stack = m_stack;
            if (stack.Count > 0)
            {
                return ((StackFrame)(stack.Pop())).Message;
            }
            return "";
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
        /// using(log4net.ThreadContext.Stacks["NDC"].Push("Stack_Message"))
        /// {
        ///		log.Warn("This should have an ThreadContext Stack message");
        ///	}
        /// </code>
        /// </example>
        public IDisposable Push(string message)
        {
            Stack stack = m_stack;
            stack.Push(new StackFrame(message, (stack.Count > 0) ? (StackFrame)stack.Peek() : null));

            // 实际上是 using 之后 的 Dispose() 方法中才弹出栈，而且是直接扔掉了，接收不到
            // 大概是处理完了这个信息，就把他自动 Pop() 掉的意思吧

            // 注意 AutoPopStackFrame 是结构体，也就是说它内部维护的 stack Pop 后，当前内中的 stack 也 Pop 掉了

            return new AutoPopStackFrame(stack, stack.Count - 1);
        }

        #endregion Public Methods

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

        #region Internal Methods

        /// <summary>
        /// 把栈中的所有信息 Message 都递归返回
        /// </summary>
        /// <returns></returns>
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
        /// 表示栈中压入的对象，内部类（链式结构）
        /// </summary>
        private sealed class StackFrame
        {
            #region Private Instance Fields

            private readonly string m_message;
            private readonly StackFrame m_parent;
            private string m_fullMessage = null;

            #endregion

            #region Internal Instance Constructors

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

            internal string Message
            {
                get { return m_message; }
            }

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
        /// Struct returned from the <see cref="ThreadContextStack.Push"/> method.
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
            /// The ThreadContextStack internal stack
            /// </summary>
            private Stack m_frameStack;

            /// <summary>
            /// 释放此实例时，将栈修剪到的深度
            /// 意思是留几个，剩下的全部 Pop()
            /// </summary>
            private int m_frameDepth;

            #endregion Private Instance Fields

            #region Internal Instance Constructors

            internal AutoPopStackFrame(Stack frameStack, int frameDepth)
            {
                m_frameStack = frameStack;
                m_frameDepth = frameDepth;
            }

            #endregion Internal Instance Constructors

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (m_frameDepth >= 0 && m_frameStack != null)
                {
                    while (m_frameStack.Count > m_frameDepth)
                    {
                        m_frameStack.Pop();
                    }
                }
            }

            #endregion Implementation of IDisposable
        }
    }
}
