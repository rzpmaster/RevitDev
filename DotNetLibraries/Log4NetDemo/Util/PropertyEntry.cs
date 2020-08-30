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
}
