using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Layout.RawLayout
{
    class RawPropertyLayout : IRawLayout
    {
        private string m_key;
        public string Key
        {
            get { return m_key; }
            set { m_key = value; }
        }

        public object Format(LoggingEvent loggingEvent)
        {
            return loggingEvent.LookupProperty(m_key);
        }
    }
}
