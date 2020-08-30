using Log4NetDemo.Appender;
using Log4NetDemo.Appender.ErrorHandler;
using Log4NetDemo.Configration;
using Log4NetDemo.Core;
using Log4NetDemo.Layout;
using Log4NetDemo.Repository;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Log4NetDemo.Test.Appender
{
    [TestFixture]
    class DebugAppenderTest
    {
        [Test]
        public void NullCategoryTest()
        {
            CategoryTraceListener categoryTraceListener = new CategoryTraceListener();
            Debug.Listeners.Add(categoryTraceListener);

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            DebugAppender debugAppender = new DebugAppender();
            debugAppender.Layout = new SimpleLayout();
            debugAppender.ActivateOptions();

            debugAppender.Category = null;

            TestErrorHandler testErrHandler = new TestErrorHandler();
            debugAppender.ErrorHandler = testErrHandler;

            BasicConfigurator.Configure(rep, debugAppender);

            ILog log = LogManager.GetLogger(rep.Name, GetType());
            log.Debug("Message");

            Assert.AreEqual(
                null,
                categoryTraceListener.Category);

            Assert.IsFalse(testErrHandler.ErrorOccured);

            Debug.Listeners.Remove(categoryTraceListener);
        }

        [Test]
        public void EmptyStringCategoryTest()
        {
            CategoryTraceListener categoryTraceListener = new CategoryTraceListener();
            Debug.Listeners.Add(categoryTraceListener);

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            DebugAppender debugAppender = new DebugAppender();
            debugAppender.Layout = new SimpleLayout();
            debugAppender.ActivateOptions();

            debugAppender.Category = new PatternLayout("");

            BasicConfigurator.Configure(rep, debugAppender);

            ILog log = LogManager.GetLogger(rep.Name, GetType());
            log.Debug("Message");

            Assert.AreEqual(
                null,
                categoryTraceListener.Category);

            Debug.Listeners.Remove(categoryTraceListener);
        }

        [Test]
        public void DefaultCategoryTest()
        {
            CategoryTraceListener categoryTraceListener = new CategoryTraceListener();
            Debug.Listeners.Add(categoryTraceListener);

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            DebugAppender debugAppender = new DebugAppender();
            debugAppender.Layout = new SimpleLayout();
            debugAppender.ActivateOptions();

            BasicConfigurator.Configure(rep, debugAppender);

            ILog log = LogManager.GetLogger(rep.Name, GetType());
            log.Debug("Message");

            Assert.AreEqual(
                GetType().ToString(),
                categoryTraceListener.Category);

            Debug.Listeners.Remove(categoryTraceListener);
        }

        [Test]
        public void MethodNameCategoryTest()
        {
            CategoryTraceListener categoryTraceListener = new CategoryTraceListener();
            Debug.Listeners.Add(categoryTraceListener);

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            DebugAppender debugAppender = new DebugAppender();
            PatternLayout methodLayout = new PatternLayout("%method");
            methodLayout.ActivateOptions();
            debugAppender.Category = methodLayout;
            debugAppender.Layout = new SimpleLayout();
            debugAppender.ActivateOptions();

            BasicConfigurator.Configure(rep, debugAppender);

            ILog log = LogManager.GetLogger(rep.Name, GetType());
            log.Debug("Message");

            Assert.AreEqual(
                System.Reflection.MethodInfo.GetCurrentMethod().Name,
                categoryTraceListener.Category);

            Debug.Listeners.Remove(categoryTraceListener);
        }

        private class TestErrorHandler : IErrorHandler
        {
            private bool m_errorOccured = false;

            public bool ErrorOccured
            {
                get { return m_errorOccured; }
            }
            #region IErrorHandler Members

            public void Error(string message, Exception e, ErrorCode errorCode)
            {
                m_errorOccured = true;
            }

            public void Error(string message, Exception e)
            {
                Error(message, e, ErrorCode.GenericFailure);
            }

            public void Error(string message)
            {
                Error(message, null, ErrorCode.GenericFailure);
            }

            #endregion
        }
    }
}
