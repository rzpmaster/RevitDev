using System;
using Log4NetDemo.Appender;
using Log4NetDemo.Appender.Interface.Evaluator;
using Log4NetDemo.Configration;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Repository.Hierarchy;
using Log4NetDemo.Test.Appender.TestAppender;
using NUnit.Framework;

namespace Log4NetDemo.Test.Core
{
    /// <summary>
    /// 测试 ITriggeringEventEvaluator
    /// </summary>
    [TestFixture]
    public class EvaluatorTest
    {
        private BufferingForwardingAppender m_bufferingForwardingAppender;
        private CountingAppender m_countingAppender;
        private Hierarchy m_hierarchy;

        [SetUp]
        public void SetupRepository()
        {
            m_hierarchy = new Hierarchy();

            m_countingAppender = new CountingAppender();
            m_countingAppender.ActivateOptions();

            m_bufferingForwardingAppender = new BufferingForwardingAppender();
            m_bufferingForwardingAppender.AddAppender(m_countingAppender);

            m_bufferingForwardingAppender.BufferSize = 5;
            m_bufferingForwardingAppender.ClearFilters();
            m_bufferingForwardingAppender.Fix = FixFlags.Partial;
            m_bufferingForwardingAppender.Lossy = false;
            m_bufferingForwardingAppender.LossyEvaluator = null;
            m_bufferingForwardingAppender.Threshold = Level.All;
        }

        [Test]
        public void TestLevelEvaluator()
        {
            m_bufferingForwardingAppender.Evaluator = new LevelEvaluator(Level.Info);
            m_bufferingForwardingAppender.ActivateOptions();
            BasicConfigurator.Configure(m_hierarchy, m_bufferingForwardingAppender);

            ILogger logger = m_hierarchy.GetLogger("TestLevelEvaluator");

            logger.Log(typeof(EvaluatorTest), Level.Debug, "Debug message logged", null);
            logger.Log(typeof(EvaluatorTest), Level.Debug, "Debug message logged", null);
            Assert.AreEqual(0, m_countingAppender.Counter, "Test 2 events buffered");

            logger.Log(typeof(EvaluatorTest), Level.Info, "Info message logged", null);
            Assert.AreEqual(3, m_countingAppender.Counter, "Test 3 events flushed on Info message.");
        }

        [Test]
        public void TestExceptionEvaluator()
        {
            m_bufferingForwardingAppender.Evaluator = new ExceptionEvaluator(typeof(ApplicationException), true);
            m_bufferingForwardingAppender.ActivateOptions();
            BasicConfigurator.Configure(m_hierarchy, m_bufferingForwardingAppender);

            ILogger logger = m_hierarchy.GetLogger("TestExceptionEvaluator");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            Assert.AreEqual(0, m_countingAppender.Counter, "Test 2 events buffered");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", new ApplicationException());
            Assert.AreEqual(3, m_countingAppender.Counter, "Test 3 events flushed on ApplicationException message.");
        }

        [Test]
        public void TestExceptionEvaluatorTriggerOnSubClass()
        {
            m_bufferingForwardingAppender.Evaluator = new ExceptionEvaluator(typeof(Exception), true);
            m_bufferingForwardingAppender.ActivateOptions();
            BasicConfigurator.Configure(m_hierarchy, m_bufferingForwardingAppender);

            ILogger logger = m_hierarchy.GetLogger("TestExceptionEvaluatorTriggerOnSubClass");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            Assert.AreEqual(0, m_countingAppender.Counter, "Test 2 events buffered");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", new ApplicationException());
            Assert.AreEqual(3, m_countingAppender.Counter, "Test 3 events flushed on ApplicationException message.");
        }

        [Test]
        public void TestExceptionEvaluatorNoTriggerOnSubClass()
        {
            m_bufferingForwardingAppender.Evaluator = new ExceptionEvaluator(typeof(Exception), false);
            m_bufferingForwardingAppender.ActivateOptions();
            BasicConfigurator.Configure(m_hierarchy, m_bufferingForwardingAppender);

            ILogger logger = m_hierarchy.GetLogger("TestExceptionEvaluatorNoTriggerOnSubClass");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            Assert.AreEqual(0, m_countingAppender.Counter, "Test 2 events buffered");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", new ApplicationException());
            Assert.AreEqual(0, m_countingAppender.Counter, "Test 3 events buffered");
        }

        [Test]
        public void TestInvalidExceptionEvaluator()
        {
            // warning: String is not a subclass of Exception
            m_bufferingForwardingAppender.Evaluator = new ExceptionEvaluator(typeof(String), false);
            m_bufferingForwardingAppender.ActivateOptions();
            BasicConfigurator.Configure(m_hierarchy, m_bufferingForwardingAppender);

            ILogger logger = m_hierarchy.GetLogger("TestExceptionEvaluatorNoTriggerOnSubClass");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", null);
            Assert.AreEqual(0, m_countingAppender.Counter, "Test 2 events buffered");

            logger.Log(typeof(EvaluatorTest), Level.Warn, "Warn message logged", new ApplicationException());
            Assert.AreEqual(0, m_countingAppender.Counter, "Test 3 events buffered");
        }
    }
}
