using Log4NetDemo.Appender;
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
    class TraceAppenderTest
    {
        [Test]
        public void DefaultCategoryTest()
        {
            CategoryTraceListener categoryTraceListener = new CategoryTraceListener();
            Trace.Listeners.Clear();
            Trace.Listeners.Add(categoryTraceListener);

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            TraceAppender traceAppender = new TraceAppender();
            traceAppender.Layout = new SimpleLayout();
            traceAppender.ActivateOptions();

            BasicConfigurator.Configure(rep, traceAppender);

            ILog log = LogManager.GetLogger(rep.Name, GetType());
            log.Debug("Message");

            Assert.AreEqual(
                GetType().ToString(),
                categoryTraceListener.Category);
        }

        [Test]
        public void MethodNameCategoryTest()
        {
            CategoryTraceListener categoryTraceListener = new CategoryTraceListener();
            Trace.Listeners.Clear();
            Trace.Listeners.Add(categoryTraceListener);

            ILoggerRepository rep = LogManager.CreateRepository(Guid.NewGuid().ToString());

            TraceAppender traceAppender = new TraceAppender();
            PatternLayout methodLayout = new PatternLayout("%method");
            methodLayout.ActivateOptions();
            traceAppender.Category = methodLayout;
            traceAppender.Layout = new SimpleLayout();
            traceAppender.ActivateOptions();

            BasicConfigurator.Configure(rep, traceAppender);

            ILog log = LogManager.GetLogger(rep.Name, GetType());
            log.Debug("Message");

            Assert.AreEqual(
                System.Reflection.MethodInfo.GetCurrentMethod().Name,
                categoryTraceListener.Category);
        }
    }

    public class CategoryTraceListener : TraceListener
    {
        private string lastCategory;

        public override void Write(string message)
        {
            // empty
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void Write(string message, string category)
        {
            lastCategory = category;
            base.Write(message, category);
        }

        public string Category
        {
            get { return lastCategory; }
        }
    }
}
