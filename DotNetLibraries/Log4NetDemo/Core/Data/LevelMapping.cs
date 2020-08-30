using Log4NetDemo.Core.Interface;
using System;
using System.Collections;

namespace Log4NetDemo.Core.Data
{
    public sealed class LevelMapping : IOptionHandler
    {
        public LevelMapping()
        {
        }

        public void Add(LevelMappingEntry entry)
        {
            if (m_entriesMap.ContainsKey(entry.Level))
            {
                m_entriesMap.Remove(entry.Level);
            }
            m_entriesMap.Add(entry.Level, entry);
        }

        public LevelMappingEntry Lookup(Level level)
        {
            if (m_entries != null)
            {
                foreach (LevelMappingEntry entry in m_entries)
                {
                    if (level >= entry.Level)
                    {
                        return entry;
                    }
                }
            }
            return null;
        }

        #region IOptionHandler Members

        public void ActivateOptions()
        {
            Level[] sortKeys = new Level[m_entriesMap.Count];
            LevelMappingEntry[] sortValues = new LevelMappingEntry[m_entriesMap.Count];

            m_entriesMap.Keys.CopyTo(sortKeys, 0);
            m_entriesMap.Values.CopyTo(sortValues, 0);

            // Sort in level order
            Array.Sort(sortKeys, sortValues, 0, sortKeys.Length, null);

            // Reverse list so that highest level is first
            Array.Reverse(sortValues, 0, sortValues.Length);

            foreach (LevelMappingEntry entry in sortValues)
            {
                entry.ActivateOptions();
            }

            m_entries = sortValues;
        }

        #endregion

        private Hashtable m_entriesMap = new Hashtable();
        private LevelMappingEntry[] m_entries = null;

    }
}
