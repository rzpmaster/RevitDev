namespace Log4NetDemo.Core.Data
{
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
