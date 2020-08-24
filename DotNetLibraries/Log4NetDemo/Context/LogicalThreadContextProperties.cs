using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;
using System;
using System.Runtime.Remoting.Messaging;
using System.Security;

namespace Log4NetDemo.Context
{
    public sealed class LogicalThreadContextProperties : ContextPropertiesBase
    {
        private const string c_SlotName = "log4net.Util.LogicalThreadContextProperties";

        /// <summary>
        /// Flag used to disable this context if we don't have permission to access the CallContext.
        /// 如果没有访问 CallContext 的权限，则禁用掉此上下文
        /// </summary>
        private bool m_disabled = false;

        internal LogicalThreadContextProperties() { }

        #region Public Instance Properties

        /// <summary>
        /// 重写的索引器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        override public object this[string key]
        {
            get
            {
                // Don't create the dictionary if it does not already exist
                PropertiesDictionary dictionary = GetProperties(false);
                if (dictionary != null)
                {
                    return dictionary[key];
                }
                return null;
            }
            set
            {
                // Force the dictionary to be created
                PropertiesDictionary props = GetProperties(true);
                // Reason for cloning the dictionary below: object instances set on the CallContext
                // need to be immutable to correctly flow through async/await
                PropertiesDictionary immutableProps = new PropertiesDictionary(props);
                immutableProps[key] = value;
                SetLogicalProperties(immutableProps);
            }
        }

        #endregion Public Instance Properties

        #region Public Instance Methods

        /// <summary>
        /// Remove a property
        /// </summary>
        /// <param name="key">the key for the entry to remove</param>
        /// <remarks>
        /// <para>
        /// Remove the value for the specified <paramref name="key"/> from the context.
        /// </para>
        /// </remarks>
        public void Remove(string key)
        {
            PropertiesDictionary dictionary = GetProperties(false);
            if (dictionary != null)
            {
                PropertiesDictionary immutableProps = new PropertiesDictionary(dictionary);
                immutableProps.Remove(key);
                SetLogicalProperties(immutableProps);
            }
        }

        /// <summary>
        /// Clear all the context properties
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clear all the context properties
        /// </para>
        /// </remarks>
        public void Clear()
        {
            PropertiesDictionary dictionary = GetProperties(false);
            if (dictionary != null)
            {
                PropertiesDictionary immutableProps = new PropertiesDictionary();
                SetLogicalProperties(immutableProps);
            }
        }

        #endregion Public Instance Methods

        #region Internal Instance Methods

        /// <summary>
        /// Get the PropertiesDictionary stored in the LocalDataStoreSlot for this thread.
        /// </summary>
        /// <param name="create">create the dictionary if it does not exist, otherwise return null if is does not exist</param>
        /// <returns>the properties for this thread</returns>
        /// <remarks>
        /// <para>
        /// The collection returned is only to be used on the calling thread. If the
        /// caller needs to share the collection between different threads then the 
        /// caller must clone the collection before doings so.
        /// </para>
        /// <para>
        /// 封装了一层从 CallContext 拿取数据（get）的方法，并包装了异常处理 
        /// </para>
        /// </remarks>
        internal PropertiesDictionary GetProperties(bool create)
        {
            if (!m_disabled)
            {
                try
                {
                    PropertiesDictionary properties = GetLogicalProperties();
                    if (properties == null && create)
                    {
                        properties = new PropertiesDictionary();
                        SetLogicalProperties(properties);
                    }
                    return properties;
                }
                catch (SecurityException secEx)
                {
                    m_disabled = true;

                    // Thrown if we don't have permission to read or write the CallContext
                    LogLog.Warn(declaringType, "SecurityException while accessing CallContext. Disabling LogicalThreadContextProperties", secEx);
                }
            }

            // Only get here is we are disabled because of a security exception
            if (create)
            {
                return new PropertiesDictionary();
            }
            return null;
        }

        #endregion Internal Instance Methods

        #region Private Static Methods

        /// <summary>
        /// Gets the call context get data.
        /// </summary>
        /// <returns>The peroperties dictionary stored in the call context</returns>
        /// <remarks>
        /// The <see cref="CallContext"/> method <see cref="CallContext.GetData"/> has a
        /// security link demand, therfore we must put the method call in a seperate method
        /// that we can wrap in an exception handler.
        /// </remarks>
        private static PropertiesDictionary GetLogicalProperties()
        {
            return CallContext.GetData(c_SlotName) as PropertiesDictionary;
        }

        /// <summary>
        /// Sets the call context data.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <remarks>
        /// The <see cref="CallContext"/> method <see cref="CallContext.SetData"/> has a
        /// security link demand, therfore we must put the method call in a seperate method
        /// that we can wrap in an exception handler.
        /// </remarks>
        private static void SetLogicalProperties(PropertiesDictionary properties)
        {
            CallContext.SetData(c_SlotName, properties);
        }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the LogicalThreadContextProperties class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(LogicalThreadContextProperties);

        #endregion Private Static Fields
    }
}
