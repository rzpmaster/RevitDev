using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using Log4NetDemo.Util.TextWriters;
using System;
using System.IO;
using System.Xml;

namespace Log4NetDemo.Layout.XmlLayout
{
    abstract public class XmlLayoutBase : LayoutSkeleton
    {
        protected XmlLayoutBase() : this(false)
        {
            IgnoresException = false;
        }

        protected XmlLayoutBase(bool locationInfo)
        {
            IgnoresException = false;
            m_locationInfo = locationInfo;
        }

        public bool LocationInfo
        {
            get { return m_locationInfo; }
            set { m_locationInfo = value; }
        }

        public string InvalidCharReplacement
        {
            get { return m_invalidCharReplacement; }
            set { m_invalidCharReplacement = value; }
        }

        abstract protected void FormatXml(XmlWriter writer, LoggingEvent loggingEvent);

        #region Override implementation of LayoutSkeleton

        override public string ContentType
        {
            get { return "text/xml"; }
        }

        override public void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }
            XmlTextWriter xmlWriter = new XmlTextWriter(new ProtectCloseTextWriter(writer));
            xmlWriter.Formatting = Formatting.None;
            xmlWriter.Namespaces = false;// Write the event to the writer
            FormatXml(xmlWriter, loggingEvent);

            xmlWriter.WriteWhitespace(SystemInfo.NewLine);

            // Close on xmlWriter will ensure xml is flushed
            // the protected writer will ignore the actual close
            xmlWriter.Close();
        }

        #endregion

        #region Implementation of IOptionHandler

        override public void ActivateOptions()
        {
            // nothing to do
        }

        #endregion

        private bool m_locationInfo = false;
        private string m_invalidCharReplacement = "?";
    }
}
