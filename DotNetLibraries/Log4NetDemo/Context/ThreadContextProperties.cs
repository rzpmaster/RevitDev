using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;
using System;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 线程属性字典
    /// </summary>
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

        internal ThreadContextProperties()
        {
        }

        #endregion Public Instance Constructors

        #region Public Instance Properties

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

        public void Remove(string key)
        {
            if (_dictionary != null)
            {
                _dictionary.Remove(key);
            }
        }

        public string[] GetKeys()
        {
            if (_dictionary != null)
            {
                return _dictionary.GetKeys();
            }
            return null;
        }

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
        /// 返回的 _dictionary 只能用于调用线程。
        /// 如果调用方需要在不同线程之间共享 _dictionary ，调用方必须先克隆它
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
