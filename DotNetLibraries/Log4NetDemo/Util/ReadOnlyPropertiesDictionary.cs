using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml;

namespace Log4NetDemo.Util
{
    /// <summary>
    /// key: string value: object
    /// 只读
    /// </summary>
    [Serializable]
    public class ReadOnlyPropertiesDictionary : ISerializable, IDictionary, ICollection, IEnumerable
    {
        public ReadOnlyPropertiesDictionary() { }

        public ReadOnlyPropertiesDictionary(ReadOnlyPropertiesDictionary propertiesDictionary)
        {
            foreach (DictionaryEntry entry in propertiesDictionary)
            {
                InnerHashtable.Add(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// 反序列化构造函数
        /// </summary>
        /// <param name="info">序列化对象数据</param>
        /// <param name="context">包含有关源或目标的上下文信息</param>
        protected ReadOnlyPropertiesDictionary(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry entry in info)
            {
                // The keys are stored as Xml encoded names
                InnerHashtable[XmlConvert.DecodeName(entry.Name)] = entry.Value;
            }
        }

        private readonly Hashtable m_hashtable = new Hashtable();
        /// <summary>
        /// 内部维护的哈希表
        /// </summary>
        protected Hashtable InnerHashtable
        {
            get { return m_hashtable; }
        }

        #region Public Instance Methods

        public string[] GetKeys()
        {
            string[] keys = new String[InnerHashtable.Count];
            InnerHashtable.Keys.CopyTo(keys, 0);
            return keys;
        }

        public virtual object this[string key]
        {
            get { return InnerHashtable[key]; }
            set { throw new NotSupportedException("This is a Read Only Dictionary and can not be modified"); }
        }

        public bool Contains(string key)
        {
            return InnerHashtable.Contains(key);
        }

        #endregion

        #region Implementation of IDictionary

        bool IDictionary.IsReadOnly => true;

        bool IDictionary.IsFixedSize => InnerHashtable.IsFixedSize;

        ICollection IDictionary.Keys => InnerHashtable.Keys;

        ICollection IDictionary.Values => InnerHashtable.Values;

        object IDictionary.this[object key]
        {
            get
            {
                if (!(key is string)) throw new ArgumentException("key must be a string");
                return InnerHashtable[key];
            }
            set
            {
                throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
            }
        }

        void IDictionary.Add(object key, object value)
        {
            throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
        }

        public virtual void Clear()
        {
            throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
        }

        void IDictionary.Remove(object key)
        {
            throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
        }

        bool IDictionary.Contains(object key)
        {
            return InnerHashtable.Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return InnerHashtable.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection

        public int Count => InnerHashtable.Count;

        bool ICollection.IsSynchronized => InnerHashtable.IsSynchronized;

        object ICollection.SyncRoot => InnerHashtable.SyncRoot;

        void ICollection.CopyTo(Array array, int index)
        {
            InnerHashtable.CopyTo(array, index);
        }

        #endregion

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)InnerHashtable).GetEnumerator();
        }

        #endregion

        #region Implementation of ISerializable

        /// <summary>
        /// 将对象序列化为<see cref="SerializationInfo" />
        /// </summary>
        /// <param name="info">序列化后 填充<see cref="SerializationInfo" /> </param>
        /// <param name="context">序列化的目标</param>
        [System.Security.SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (DictionaryEntry entry in InnerHashtable.Clone() as IDictionary)
            {
                string entryKey = entry.Key as string;
                object entryValue = entry.Value;

                // If value is serializable then we add it to the list
                bool isSerializable = entryValue.GetType().IsSerializable;
                if (entryKey != null && entryValue != null && isSerializable)
                {
                    // Store the keys as an Xml encoded local name as it may contain colons (':') 
                    // which are NOT escaped by the Xml Serialization framework.
                    // This must be a bug in the serialization framework as we cannot be expected
                    // to know the implementation details of all the possible transport layers.
                    info.AddValue(XmlConvert.EncodeLocalName(entryKey), entryValue);
                }
            }
        }

        #endregion


    }
}
