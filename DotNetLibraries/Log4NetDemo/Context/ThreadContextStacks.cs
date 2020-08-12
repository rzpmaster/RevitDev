using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 线程属性字典维护的 栈集合
    /// </summary>
    public sealed class ThreadContextStacks
    {
        /// <summary>
        /// 内部维护的属性字典，键是 string 值是 ThreadContextStack
        /// </summary>
        private readonly ContextPropertiesBase m_properties;

        internal ThreadContextStacks(ContextPropertiesBase properties)
        {
            m_properties = properties;
        }

        /// <summary>
        /// 线程栈索引器
        /// 只读，如果键没找到，会自动添加一个键，然后返回一个空栈
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ThreadContextStack this[string key]
        {
            get
            {
                ThreadContextStack stack = null;

                // 取值的时候，先去属性字典里取，
                // 如果属性字典没有，就创建一个存储栈，并在属性字典中加入
                // 如果已经在属性字典里了，就把他强转成存储栈返回

                object propertyValue = m_properties[key];
                if (propertyValue == null)
                {
                    // Stack does not exist, create
                    stack = new ThreadContextStack();
                    m_properties[key] = stack;
                }
                else
                {
                    // Look for existing stack
                    stack = propertyValue as ThreadContextStack;
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

                        stack = new ThreadContextStack();
                    }
                }

                return stack;
            }
        }

        private readonly static Type declaringType = typeof(ThreadContextStacks);
    }
}
