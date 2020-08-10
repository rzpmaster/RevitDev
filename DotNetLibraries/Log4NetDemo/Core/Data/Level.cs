using System;

namespace Log4NetDemo.Core.Data
{
    /// <summary>
    /// 日志等级级别
    /// </summary>
    [Serializable]
    public sealed class Level : IComparable, IComparable<Level>
    {
        public readonly static Level Off = new Level(int.MaxValue, "OFF");
        public readonly static Level Log4Net_Debug = new Level(120000, "log4net:DEBUG");
        public readonly static Level Emergency = new Level(120000, "EMERGENCY");
        public readonly static Level Fatal = new Level(110000, "FATAL");
        public readonly static Level Alert = new Level(100000, "ALERT");
        public readonly static Level Critical = new Level(90000, "CRITICAL");
        public readonly static Level Severe = new Level(80000, "SEVERE");
        public readonly static Level Error = new Level(70000, "ERROR");
        public readonly static Level Warn = new Level(60000, "WARN");
        public readonly static Level Notice = new Level(50000, "NOTICE");
        public readonly static Level Info = new Level(40000, "INFO");
        public readonly static Level Debug = new Level(30000, "DEBUG");
        public readonly static Level Fine = new Level(30000, "FINE");
        public readonly static Level Trace = new Level(20000, "TRACE");
        public readonly static Level Finer = new Level(20000, "FINER");
        public readonly static Level Verbose = new Level(10000, "VERBOSE");
        public readonly static Level Finest = new Level(10000, "FINEST");
        public readonly static Level All = new Level(int.MinValue, "ALL");

        #region Public Instance Constructors

        public Level(int level, string levelName, string displayName)
        {
            if (levelName == null)
            {
                throw new ArgumentNullException("levelName");
            }
            if (displayName == null)
            {
                throw new ArgumentNullException("displayName");
            }

            m_levelValue = level;
            m_levelName = string.Intern(levelName);
            m_levelDisplayName = displayName;
        }

        public Level(int level, string levelName) : this(level, levelName, levelName)
        {
        }

        #endregion

        #region Public Instance Properties

        public string Name
        {
            get { return m_levelName; }
        }

        public int Value
        {
            get { return m_levelValue; }
        }

        public string DisplayName
        {
            get { return m_levelDisplayName; }
        }

        #endregion

        #region Override implementation of Object

        override public string ToString()
        {
            return m_levelName;
        }

        override public bool Equals(object o)
        {
            Level otherLevel = o as Level;
            if (otherLevel != null)
            {
                return m_levelValue == otherLevel.m_levelValue;
            }
            else
            {
                return base.Equals(o);
            }
        }

        override public int GetHashCode()
        {
            return m_levelValue;
        }

        #endregion

        #region Implementation of IComparable

        public int CompareTo(object obj)
        {
            Level target = obj as Level;
            if (target != null)
            {
                return this.CompareTo(target);
            }
            throw new ArgumentException("Parameter: r, Value: [" + obj + "] is not an instance of Level");
        }

        public int CompareTo(Level other)
        {
            // Reference equals
            if ((object)this == (object)other)
            {
                return 0;
            }

            if (this == null && other == null)
            {
                return 0;
            }
            if (this == null)
            {
                return -1;
            }
            if (other == null)
            {
                return 1;
            }

            return this.m_levelValue.CompareTo(other.m_levelValue);
        }

        #endregion

        #region Operators

        public static bool operator >(Level l, Level r)
        {
            return l.m_levelValue > r.m_levelValue;
        }

        public static bool operator <(Level l, Level r)
        {
            return l.m_levelValue < r.m_levelValue;
        }

        public static bool operator >=(Level l, Level r)
        {
            return l.m_levelValue >= r.m_levelValue;
        }

        public static bool operator <=(Level l, Level r)
        {
            return l.m_levelValue <= r.m_levelValue;
        }

        public static bool operator ==(Level l, Level r)
        {
            if (((object)l) != null && ((object)r) != null)
            {
                return l.m_levelValue == r.m_levelValue;
            }
            else
            {
                return ((object)l) == ((object)r);
            }
        }

        public static bool operator !=(Level l, Level r)
        {
            return !(l == r);
        }

        #endregion

        #region Private Instance Fields

        private readonly int m_levelValue;
        private readonly string m_levelName;
        private readonly string m_levelDisplayName;

        #endregion
    }
}
