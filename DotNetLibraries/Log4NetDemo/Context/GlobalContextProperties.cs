using Log4NetDemo.Util.Collections;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 全局属性字典
    /// </summary>
    public sealed class GlobalContextProperties : ContextPropertiesBase
    {
        /// <summary>
        /// Lock object used to synchronize updates within this instance
        /// </summary>
        private readonly object m_syncRoot = new object();

        internal GlobalContextProperties() { }

        /// <summary>
        /// The read only copy of the properties.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This variable is declared <c>volatile</c> to prevent the compiler and JIT from
        /// reordering reads and writes of this thread performed on different threads.
        /// </para>
        /// <para>
        /// 此变量声明为 <c>volatile</c> 以防止编译器和JIT重新排序此线程 在 不同线程平台上 执行的读写操作。
        ///</para>
        /// </remarks>
        private volatile ReadOnlyPropertiesDictionary m_readOnlyProperties = new ReadOnlyPropertiesDictionary();

        /// <summary>
        /// 重写属性索引器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        override public object this[string key]
        {
            get
            {
                return m_readOnlyProperties[key];
            }
            set
            {
                lock (m_syncRoot)
                {
                    PropertiesDictionary mutableProps = new PropertiesDictionary(m_readOnlyProperties);

                    mutableProps[key] = value;

                    m_readOnlyProperties = new ReadOnlyPropertiesDictionary(mutableProps);
                }
            }
        }

        #region Public Instance Methods

        public void Remove(string key)
        {
            lock (m_syncRoot)
            {
                if (m_readOnlyProperties.Contains(key))
                {
                    PropertiesDictionary mutableProps = new PropertiesDictionary(m_readOnlyProperties);

                    mutableProps.Remove(key);

                    m_readOnlyProperties = new ReadOnlyPropertiesDictionary(mutableProps);
                }
            }
        }

        public void Clear()
        {
            lock (m_syncRoot)
            {
                m_readOnlyProperties = new ReadOnlyPropertiesDictionary();
            }
        }

        #endregion Public Instance Methods

        #region Internal Instance Methods

        internal ReadOnlyPropertiesDictionary GetReadOnlyProperties()
        {
            return m_readOnlyProperties;
        }

        #endregion Internal Instance Methods
    }
}
