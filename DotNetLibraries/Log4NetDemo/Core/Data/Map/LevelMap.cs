using Log4NetDemo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Core.Data.Map
{
    /// <summary>
    /// Mapping between string name and Level object
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mapping between string name and <see cref="Level"/> object.
    /// This mapping is held separately for each <see cref="Repository.ILoggerRepository"/>.
    /// The level name is case insensitive.
    /// </para>
    /// </remarks>
    public sealed class LevelMap
    {
        public LevelMap() { }

        public void Clear()
        {
            // Clear all current levels
            m_mapName2Level.Clear();
        }

        public Level this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                lock (this)
                {
                    return (Level)m_mapName2Level[name];
                }
            }
        }

        public void Add(string name, int value)
        {
            Add(name, value, null);
        }

        public void Add(string name, int value, string displayName)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw Util.SystemInfo.CreateArgumentOutOfRangeException("name", name, "Parameter: name, Value: [" + name + "] out of range. Level name must not be empty");
            }

            if (displayName == null || displayName.Length == 0)
            {
                displayName = name;
            }

            Add(new Level(value, name, displayName));
        }

        public void Add(Level level)
        {
            if (level == null)
            {
                throw new ArgumentNullException("level");
            }
            lock (this)
            {
                m_mapName2Level[level.Name] = level;
            }
        }

        public ICollection<Level> AllLevels
        {
            get
            {
                lock (this)
                {
                    return m_mapName2Level.Values.Cast<Level>().ToList();
                }
            }
        }

        public Level LookupWithDefault(Level defaultLevel)
        {
            if (defaultLevel == null)
            {
                throw new ArgumentNullException("defaultLevel");
            }

            lock (this)
            {
                Level level = (Level)m_mapName2Level[defaultLevel.Name];
                if (level == null)
                {
                    m_mapName2Level[defaultLevel.Name] = defaultLevel;
                    return defaultLevel;
                }
                return level;
            }
        }

        /// <summary>
        /// 内部维护的一个哈希表
        /// </summary>
        private Hashtable m_mapName2Level = SystemInfo.CreateCaseInsensitiveHashtable();
    }
}
