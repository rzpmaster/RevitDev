using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;
using System;
using System.Text;
using System.Xml;

namespace Log4NetDemo.Layout.XmlLayout
{
    public class XmlLayout : XmlLayoutBase
    {
        public XmlLayout() : base()
        {
        }

        public XmlLayout(bool locationInfo) : base(locationInfo)
        {
        }

        public string Prefix
        {
            get { return m_prefix; }
            set { m_prefix = value; }
        }

        public bool Base64EncodeMessage
        {
            get { return m_base64Message; }
            set { m_base64Message = value; }
        }

        public bool Base64EncodeProperties
        {
            get { return m_base64Properties; }
            set { m_base64Properties = value; }
        }

        override public void ActivateOptions()
        {
            base.ActivateOptions();

            // Cache the full element names including the prefix
            if (m_prefix != null && m_prefix.Length > 0)
            {
                m_elmEvent = m_prefix + ":" + ELM_EVENT;
                m_elmMessage = m_prefix + ":" + ELM_MESSAGE;
                m_elmProperties = m_prefix + ":" + ELM_PROPERTIES;
                m_elmData = m_prefix + ":" + ELM_DATA;
                m_elmException = m_prefix + ":" + ELM_EXCEPTION;
                m_elmLocation = m_prefix + ":" + ELM_LOCATION;
            }
        }

        override protected void FormatXml(XmlWriter writer, LoggingEvent loggingEvent)
        {
            writer.WriteStartElement(m_elmEvent);
            writer.WriteAttributeString(ATTR_LOGGER, loggingEvent.LoggerName);
            writer.WriteAttributeString(ATTR_TIMESTAMP, XmlConvert.ToString(loggingEvent.TimeStamp, XmlDateTimeSerializationMode.Local));
            writer.WriteAttributeString(ATTR_LEVEL, loggingEvent.Level.DisplayName);
            writer.WriteAttributeString(ATTR_THREAD, loggingEvent.ThreadName);

            if (loggingEvent.Domain != null && loggingEvent.Domain.Length > 0)
            {
                writer.WriteAttributeString(ATTR_DOMAIN, loggingEvent.Domain);
            }
            if (loggingEvent.Identity != null && loggingEvent.Identity.Length > 0)
            {
                writer.WriteAttributeString(ATTR_IDENTITY, loggingEvent.Identity);
            }
            if (loggingEvent.UserName != null && loggingEvent.UserName.Length > 0)
            {
                writer.WriteAttributeString(ATTR_USERNAME, loggingEvent.UserName);
            }

            // Append the message text
            writer.WriteStartElement(m_elmMessage);
            if (!this.Base64EncodeMessage)
            {
                Transform.WriteEscapedXmlString(writer, loggingEvent.RenderedMessage, this.InvalidCharReplacement);
            }
            else
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(loggingEvent.RenderedMessage);
                string base64Message = Convert.ToBase64String(messageBytes, 0, messageBytes.Length);
                Transform.WriteEscapedXmlString(writer, base64Message, this.InvalidCharReplacement);
            }
            writer.WriteEndElement();

            PropertiesDictionary properties = loggingEvent.GetProperties();

            // Append the properties text
            if (properties.Count > 0)
            {
                writer.WriteStartElement(m_elmProperties);
                foreach (System.Collections.DictionaryEntry entry in properties)
                {
                    writer.WriteStartElement(m_elmData);
                    writer.WriteAttributeString(ATTR_NAME, Transform.MaskXmlInvalidCharacters((string)entry.Key, this.InvalidCharReplacement));

                    // Use an ObjectRenderer to convert the object to a string
                    string valueStr = null;
                    if (!this.Base64EncodeProperties)
                    {
                        valueStr = Transform.MaskXmlInvalidCharacters(loggingEvent.Repository.RendererMap.FindAndRender(entry.Value), this.InvalidCharReplacement);
                    }
                    else
                    {
                        byte[] propertyValueBytes = Encoding.UTF8.GetBytes(loggingEvent.Repository.RendererMap.FindAndRender(entry.Value));
                        valueStr = Convert.ToBase64String(propertyValueBytes, 0, propertyValueBytes.Length);
                    }
                    writer.WriteAttributeString(ATTR_VALUE, valueStr);

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            string exceptionStr = loggingEvent.GetExceptionString();
            if (exceptionStr != null && exceptionStr.Length > 0)
            {
                // Append the stack trace line
                writer.WriteStartElement(m_elmException);
                Transform.WriteEscapedXmlString(writer, exceptionStr, this.InvalidCharReplacement);
                writer.WriteEndElement();
            }

            if (LocationInfo)
            {
                LocationInfo locationInfo = loggingEvent.LocationInformation;

                writer.WriteStartElement(m_elmLocation);
                writer.WriteAttributeString(ATTR_CLASS, locationInfo.ClassName);
                writer.WriteAttributeString(ATTR_METHOD, locationInfo.MethodName);
                writer.WriteAttributeString(ATTR_FILE, locationInfo.FileName);
                writer.WriteAttributeString(ATTR_LINE, locationInfo.LineNumber);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private string m_prefix = PREFIX;

        private string m_elmEvent = ELM_EVENT;
        private string m_elmMessage = ELM_MESSAGE;
        private string m_elmData = ELM_DATA;
        private string m_elmProperties = ELM_PROPERTIES;
        private string m_elmException = ELM_EXCEPTION;
        private string m_elmLocation = ELM_LOCATION;

        private bool m_base64Message = false;
        private bool m_base64Properties = false;

        #region Private Static Fields

        private const string PREFIX = "log4net";

        private const string ELM_EVENT = "event";
        private const string ELM_MESSAGE = "message";
        private const string ELM_PROPERTIES = "properties";
        private const string ELM_GLOBAL_PROPERTIES = "global-properties";
        private const string ELM_DATA = "data";
        private const string ELM_EXCEPTION = "exception";
        private const string ELM_LOCATION = "locationInfo";

        private const string ATTR_LOGGER = "logger";
        private const string ATTR_TIMESTAMP = "timestamp";
        private const string ATTR_LEVEL = "level";
        private const string ATTR_THREAD = "thread";
        private const string ATTR_DOMAIN = "domain";
        private const string ATTR_IDENTITY = "identity";
        private const string ATTR_USERNAME = "username";
        private const string ATTR_CLASS = "class";
        private const string ATTR_METHOD = "method";
        private const string ATTR_FILE = "file";
        private const string ATTR_LINE = "line";
        private const string ATTR_NAME = "name";
        private const string ATTR_VALUE = "value";


        #endregion Private Static Fields
    }
}
