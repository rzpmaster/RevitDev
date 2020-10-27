using System.Configuration;
using System.Xml;

namespace Log4NetDemo.Configration
{
    class Log4NetConfigurationSectionHandler: IConfigurationSectionHandler
    {
        public Log4NetConfigurationSectionHandler()
        {
        }

        #region Implementation of IConfigurationSectionHandler

        public object Create(object parent, object configContext, XmlNode section)
        {
            return section;
        }

        #endregion
    }
}
