using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Log4NetDemo.Util
{
    /// <summary>
    /// key: string value: object
    /// 可读可写
    /// </summary>
    [Serializable]
    public sealed class PropertiesDictionary : ReadOnlyPropertiesDictionary, ISerializable, IDictionary, ICollection, IEnumerable
    {
        public PropertiesDictionary() { }

        public PropertiesDictionary(ReadOnlyPropertiesDictionary propertiesDictionary) : base(propertiesDictionary) { }

        /// <summary>
        /// 序列化构造函数(密封类,所以是private)
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private PropertiesDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #region Public Instance Methods

        override public object this[string key]
        {
            get { return InnerHashtable[key]; }
            set { InnerHashtable[key] = value; }
        }

        public void Remove(string key)
        {
            InnerHashtable.Remove(key);
        }

        #endregion

        #region Implementation of IDictionary

        bool IDictionary.IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        object IDictionary.this[object key]
        {
            get
            {
                if (!(key is string))
                {
                    throw new ArgumentException("key must be a string", "key");
                }
                return InnerHashtable[key];
            }
            set
            {
                if (!(key is string))
                {
                    throw new ArgumentException("key must be a string", "key");
                }
                InnerHashtable[key] = value;
            }
        }

        void IDictionary.Add(object key, object value)
        {
            if (!(key is string))
            {
                throw new ArgumentException("key must be a string", "key");
            }
            InnerHashtable.Add(key, value);
        }

        public override void Clear()
        {
            InnerHashtable.Clear();
        }

        void IDictionary.Remove(object key)
        {
            InnerHashtable.Remove(key);
        }

        #endregion
    }
}
