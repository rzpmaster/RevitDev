using System;
using Log4NetDemo.Configration;
using Log4NetDemo.Core;
using Log4NetDemo.Layout;
using Log4NetDemo.Repository;
using Log4NetDemo.Test.Appender.TestAppender;
using NUnit.Framework;

namespace Log4NetDemo.Test.Core
{
    /// <summary>
    /// 测试关闭仓库后，不配置就不能使用
    /// </summary>
    [TestFixture]
    class ShutdownTest
    {
        [Test]
        public void TestShutdownAndReconfigure()
        {
            // Create unique repository
            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            // Create appender and configure repos
            StringAppender stringAppender = new StringAppender();
            stringAppender.Layout = new PatternLayout("%m");
            BasicConfigurator.Configure(rep, stringAppender);

            // Get logger from repos
            ILog log1 = LogManager.GetLogger(rep.Name, "logger1");

            log1.Info("TestMessage1");
            Assert.AreEqual("TestMessage1", stringAppender.GetString(), "Test logging configured");
            stringAppender.Reset();

            rep.Shutdown();

            log1.Info("TestMessage2");
            Assert.AreEqual("", stringAppender.GetString(), "Test not logging while shutdown");
            stringAppender.Reset();

            // Create new appender and configure
            stringAppender = new StringAppender();
            stringAppender.Layout = new PatternLayout("%m");
            BasicConfigurator.Configure(rep, stringAppender);

            log1.Info("TestMessage3");
            Assert.AreEqual("TestMessage3", stringAppender.GetString(), "Test logging re-configured");
            stringAppender.Reset();
        }
    }
}
