using Log4NetDemo.Util;

namespace Log4NetDemo.Context
{
    public sealed class GlobalContextProperties : ContextPropertiesBase
    {
        #region Private Instance Fields

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
        /// Lock object used to synchronize updates within this instance
        /// </summary>
        private readonly object m_syncRoot = new object();

        #endregion Private Instance Fields

        #region Public Instance Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="GlobalContextProperties" /> class.
        /// </para>
        /// </remarks>
        internal GlobalContextProperties()
        {
        }

        #endregion Public Instance Constructors

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the value of a property
        /// </summary>
        /// <value>
        /// The value for the property with the specified key
        /// </value>
        /// <remarks>
        /// <para>
        /// Reading the value for a key is faster than setting the value.
        /// When the value is written a new read only copy of 
        /// the properties is created.
        /// </para>
        /// </remarks>
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

        #endregion Public Instance Properties

        #region Public Instance Methods

        /// <summary>
        /// Remove a property from the global context
        /// </summary>
        /// <param name="key">the key for the entry to remove</param>
        /// <remarks>
        /// <para>
        /// Removing an entry from the global context properties is relatively expensive compared
        /// with reading a value. 
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Clear the global context properties
        /// </summary>
        public void Clear()
        {
            lock (m_syncRoot)
            {
                m_readOnlyProperties = new ReadOnlyPropertiesDictionary();
            }
        }

        #endregion Public Instance Methods

        #region Internal Instance Methods

        /// <summary>
        /// Get a readonly immutable copy of the properties
        /// </summary>
        /// <returns>the current global context properties</returns>
        /// <remarks>
        /// <para>
        /// This implementation is fast because the GlobalContextProperties class
        /// stores a readonly copy of the properties.
        /// </para>
        /// </remarks>
        internal ReadOnlyPropertiesDictionary GetReadOnlyProperties()
        {
            return m_readOnlyProperties;
        }

        #endregion Internal Instance Methods
    }
}
