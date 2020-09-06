using System;
using System.Xml;
using Log4NetDemo.Configration;
using Log4NetDemo.Core;
using Log4NetDemo.Repository;
using Log4NetDemo.Test.Appender.TestAppender;
using NUnit.Framework;

namespace Log4NetDemo.Test.Repository.Hierarchy
{
    [TestFixture]
    class HierarchyTest
    {
        [Test]
        public void SetRepositoryPropertiesInConfigFile()
        {
            // LOG4NET-53: Allow repository properties to be set in the config file
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.LoadXml(@"
                <log4net>
                  <property>
                    <key value=""two-plus-two"" />
                    <value value=""4"" />
                  </property>
                  <appender name=""StringAppender"" type=""Log4NetDemo.Test.Appender.TestAppender.StringAppender, Log4NetDemo.Test"">
                    <layout type=""Log4NetDemo.Layout.SimpleLayout"" />
                  </appender>
                  <root>
                    <level value=""ALL"" />
                    <appender-ref ref=""StringAppender"" />
                  </root>
                </log4net>");

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);

            Assert.AreEqual("4", rep.Properties["two-plus-two"]);
            Assert.IsNull(rep.Properties["one-plus-one"]);
        }

        [Test]
        public void AddingMultipleAppenders()
        {
            CountingAppender alpha = new CountingAppender();
            CountingAppender beta = new CountingAppender();

            Log4NetDemo.Repository.Hierarchy.Hierarchy hierarchy = (Log4NetDemo.Repository.Hierarchy.Hierarchy)Utils.GetRepository();

            hierarchy.Root.AddAppender(alpha);
            hierarchy.Root.AddAppender(beta);
            hierarchy.Configured = true;

            ILog log = LogManager.GetLogger(GetType());
            log.Debug("Hello World");

            Assert.AreEqual(1, alpha.Counter);
            Assert.AreEqual(1, beta.Counter);
        }

        [Test]
        public void AddingMultipleAppenders2()
        {
            CountingAppender alpha = new CountingAppender();
            CountingAppender beta = new CountingAppender();
            BasicConfigurator.Configure(alpha, beta);
            ILog log = LogManager.GetLogger(GetType());
            log.Debug("Hello World");

            Assert.AreEqual(1, alpha.Counter);
            Assert.AreEqual(1, beta.Counter);
        }

        [Test]
        public void LoggerNameCanConsistOfASingleDot()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.LoadXml(@"
                <log4net>
                  <appender name=""StringAppender"" type=""Log4NetDemo.Test.Appender.TestAppender.StringAppender, Log4NetDemo.Test"">
                    <layout type=""Log4NetDemo.Layout.SimpleLayout"" />
                  </appender>
                  <root>
                    <level value=""ALL"" />
                    <appender-ref ref=""StringAppender"" />
                  </root>
                  <logger name=""."">
                    <level value=""WARN"" />
                  </logger>
                </log4net>");

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);
        }

        [Test]
        public void LoggerNameCanConsistOfASingleNonDot()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.LoadXml(@"
                <log4net>
                  <appender name=""StringAppender"" type=""Log4NetDemo.Test.Appender.TestAppender.StringAppender, Log4NetDemo.Test"">
                    <layout type=""Log4NetDemo.Layout.SimpleLayout"" />
                  </appender>
                  <root>
                    <level value=""ALL"" />
                    <appender-ref ref=""StringAppender"" />
                  </root>
                  <logger name=""L"">
                    <level value=""WARN"" />
                  </logger>
                </log4net>");

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);
        }

        [Test]
        public void LoggerNameCanContainSequenceOfDots()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.LoadXml(@"
                <log4net>
                  <appender name=""StringAppender"" type=""Log4NetDemo.Test.Appender.TestAppender.StringAppender, Log4NetDemo.Test"">
                    <layout type=""Log4NetDemo.Layout.SimpleLayout"" />
                  </appender>
                  <root>
                    <level value=""ALL"" />
                    <appender-ref ref=""StringAppender"" />
                  </root>
                  <logger name=""L..M"">
                    <level value=""WARN"" />
                  </logger>
                </log4net>");

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());
            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);
        }
    }
}
