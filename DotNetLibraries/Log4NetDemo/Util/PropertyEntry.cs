namespace Log4NetDemo.Util
{
    public class PropertyEntry
    {
        public string Key
        {
            get { return m_key; }
            set { m_key = value; }
        }

        public object Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public override string ToString()
        {
            return "PropertyEntry(Key=" + m_key + ", Value=" + m_value + ")";
        }

        private string m_key = null;
        private object m_value = null;
    }

    internal class LevelEntry
    {
        private int m_levelValue = -1;
        private string m_levelName = null;
        private string m_levelDisplayName = null;

        public int Value
        {
            get { return m_levelValue; }
            set { m_levelValue = value; }
        }

        public string Name
        {
            get { return m_levelName; }
            set { m_levelName = value; }
        }

        public string DisplayName
        {
            get { return m_levelDisplayName; }
            set { m_levelDisplayName = value; }
        }

        public override string ToString()
        {
            return "LevelEntry(Value=" + m_levelValue + ", Name=" + m_levelName + ", DisplayName=" + m_levelDisplayName + ")";
        }
    }
}
