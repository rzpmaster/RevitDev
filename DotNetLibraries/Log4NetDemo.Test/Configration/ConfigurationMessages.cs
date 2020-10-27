using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Log4NetDemo.Appender;
using Log4NetDemo.Configration;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;
using NUnit.Framework;

namespace Log4NetDemo.Test.Configration
{
    [TestFixture]
    public class ConfigurationMessages
    {
        [Test]
        public void ConfigurationMessagesTest()
        {
            try
            {
                LogLog.EmitInternalMessages = false;
                LogLog.InternalDebugging = true;

                XmlDocument log4netConfig = new XmlDocument();
                log4netConfig.LoadXml(@"
                <log4net>
                  <appender name=""LogLogAppender"" type=""log4net.Tests.LoggerRepository.LogLogAppender, log4net.Tests"">
                    <layout type=""log4net.Layout.SimpleLayout"" />
                  </appender>
                  <appender name=""MemoryAppender"" type=""log4net.Appender.MemoryAppender"">
                    <layout type=""log4net.Layout.SimpleLayout"" />
                  </appender>
                  <root>
                    <level value=""ALL"" />
                    <appender-ref ref=""LogLogAppender"" />
                    <appender-ref ref=""MemoryAppender"" />
                  </root>  
                </log4net>");

                ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
                rep.ConfigurationChanged += new LoggerRepositoryConfigurationChangedEventHandler(rep_ConfigurationChanged);

                ICollection configurationMessages = XmlConfigurator.Configure(rep, log4netConfig["log4net"]);

                Assert.IsTrue(configurationMessages.Count > 0);
            }
            finally
            {
                LogLog.EmitInternalMessages = true;
                LogLog.InternalDebugging = false;
            }
        }

        void rep_ConfigurationChanged(object sender, EventArgs e)
        {
            ConfigurationChangedEventArgs configChanged = (ConfigurationChangedEventArgs)e;

            Assert.IsTrue(configChanged.ConfigurationMessages.Count > 0);
        }
    }

    
}
