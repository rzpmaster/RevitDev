using Log4NetDemo.Util.Collections;
using System.Collections;
using System.Collections.Generic;

namespace Log4NetDemo.Util.Collections
{
    /// <summary>
    /// 提供对嵌套属性字典的压平操作
    /// </summary>
    public sealed class CompositeProperties
    {
        internal CompositeProperties() { }

        public object this[string key]
        {
            get
            {
                // Look in the flattened properties first
                if (m_flattened != null)
                {
                    return m_flattened[key];
                }

                // Look for the key in all the nested properties
                foreach (ReadOnlyPropertiesDictionary cur in m_nestedProperties)
                {
                    if (cur.Contains(key))
                    {
                        return cur[key];
                    }
                }
                return null;
            }
        }

        public void Add(ReadOnlyPropertiesDictionary properties)
        {
            m_flattened = null;
            m_nestedProperties.Add(properties);
        }

        /// <summary>
        /// 对嵌套属性字典的压平操作
        /// </summary>
        /// <returns></returns>
        public PropertiesDictionary Flatten()
        {
            if (m_flattened == null)
            {
                m_flattened = new PropertiesDictionary();

                for (int i = m_nestedProperties.Count; --i >= 0;)
                {
                    ReadOnlyPropertiesDictionary cur = m_nestedProperties[i];

                    foreach (DictionaryEntry entry in cur)
                    {
                        m_flattened[(string)entry.Key] = entry.Value;
                    }
                }
            }
            return m_flattened;
        }


        /// <summary>
        /// 压平属性集合
        /// </summary>
        private PropertiesDictionary m_flattened = null;
        /// <summary>
        /// 嵌套属性集合 List<ReadOnlyPropertiesDictionary> 
        /// </summary>
        private List<ReadOnlyPropertiesDictionary> m_nestedProperties = new List<ReadOnlyPropertiesDictionary>();
    }
}
