using Log4NetDemo.Core.Data;
using System.IO;

namespace Log4NetDemo.Layout.RawLayout
{
    public class Layout2RawLayoutAdapter : IRawLayout
    {
        private ILayout m_layout;

        public Layout2RawLayoutAdapter(ILayout layout)
        {
            m_layout = layout;
        }

        virtual public object Format(LoggingEvent loggingEvent)
        {
            StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            m_layout.Format(writer, loggingEvent);
            return writer.ToString();
        }
    }
}
