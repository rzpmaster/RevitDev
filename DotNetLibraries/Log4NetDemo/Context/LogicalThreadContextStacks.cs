using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 逻辑线程上下文维护的 栈集合
    /// </summary>
    public sealed class LogicalThreadContextStacks
    {
        /// <summary>
        /// 内部维护的属性字典，键是 string 值是 LogicalThreadContextStack
        /// </summary>
        private readonly LogicalThreadContextProperties m_properties;

        internal LogicalThreadContextStacks(LogicalThreadContextProperties properties)
        {
            m_properties = properties;
        }

        /// <summary>
        /// 线程栈索引器
        /// 只读，如果键没找到，会自动添加一个键，然后返回一个空栈
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public LogicalThreadContextStack this[string key]
        {
            get
            {
                LogicalThreadContextStack stack = null;

                object propertyValue = m_properties[key];
                if (propertyValue == null)
                {
                    // Stack does not exist, create
                    stack = new LogicalThreadContextStack(key, new TwoArgAction(registerNew));
                    m_properties[key] = stack;
                }
                else
                {
                    // Look for existing stack
                    stack = propertyValue as LogicalThreadContextStack;
                    if (stack == null)
                    {
                        // Property is not set to a stack!
                        string propertyValueString = SystemInfo.NullText;

                        try
                        {
                            propertyValueString = propertyValue.ToString();
                        }
                        catch
                        {
                        }

                        LogLog.Error(declaringType, "ThreadContextStacks: Request for stack named [" + key + "] failed because a property with the same name exists which is a [" + propertyValue.GetType().Name + "] with value [" + propertyValueString + "]");

                        stack = new LogicalThreadContextStack(key, new TwoArgAction(registerNew));
                    }
                }

                return stack;
            }
        }

        #region Private Instance Fields

        /// <summary>
        /// 在属性字典中，添加一个新的键值对或者将旧值覆盖掉
        /// </summary>
        /// <param name="stackName"></param>
        /// <param name="stack"></param>
        private void registerNew(string stackName, LogicalThreadContextStack stack)
        {
            m_properties[stackName] = stack;
        }

        #endregion Private Instance Fields

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the ThreadContextStacks class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(LogicalThreadContextStacks);

        #endregion Private Static Fields
    }
}
