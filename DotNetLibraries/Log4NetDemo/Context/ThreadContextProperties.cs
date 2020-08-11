using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Context
{
    public sealed class ThreadContextProperties : ContextPropertiesBase
    {
        #region Private Instance Fields

        /// <summary>
        /// Each thread will automatically have its instance.
        /// </summary>
        [ThreadStatic]
        private static PropertiesDictionary _dictionary;

        #endregion Private Instance Fields

        #region Public Instance Constructors

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="ThreadContextProperties" /> class.
        /// </para>
        /// </remarks>
        internal ThreadContextProperties()
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
        /// Gets or sets the value of a property
        /// </para>
        /// </remarks>
        override public object this[string key]
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary[key];
                }
                return null;
            }
            set
            {
                GetProperties(true)[key] = value;
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
        /// Remove a property
        /// </para>
        /// </remarks>
        public void Remove(string key)
        {
            if (_dictionary != null)
            {
                _dictionary.Remove(key);
            }
        }

        /// <summary>
        /// Get the keys stored in the properties.
        /// </summary>
        /// <para>
        /// Gets the keys stored in the properties.
        /// </para>
        /// <returns>a set of the defined keys</returns>
        public string[] GetKeys()
        {
            if (_dictionary != null)
            {
                return _dictionary.GetKeys();
            }
            return null;
        }

        /// <summary>
        /// Clear all properties
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clear all properties
        /// </para>
        /// </remarks>
        public void Clear()
        {
            if (_dictionary != null)
            {
                _dictionary.Clear();
            }
        }

        #endregion Public Instance Methods

        #region Internal Instance Methods

        /// <summary>
        /// Get the <c>PropertiesDictionary</c> for this thread.
        /// </summary>
        /// <param name="create">create the dictionary if it does not exist, otherwise return null if does not exist</param>
        /// <returns>the properties for this thread</returns>
        /// <remarks>
        /// <para>
        /// The collection returned is only to be used on the calling thread. If the
        /// caller needs to share the collection between different threads then the 
        /// caller must clone the collection before doing so.
        /// </para>
        /// <para>
        /// 返回的集合只能用于调用线程。如果调用方需要在不同线程之间共享集合，然后调用方必须在执行此操作之前克隆集合。
        ///</para>
        /// </remarks>
        internal PropertiesDictionary GetProperties(bool create)
        {
            if (_dictionary == null && create)
            {
                _dictionary = new PropertiesDictionary();
            }
            return _dictionary;
        }

        #endregion Internal Instance Methods
    }
}
