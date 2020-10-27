using System;
using System.Threading;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Repository;
using NUnit.Framework;

namespace Log4NetDemo.Test.Core
{
    [TestFixture]
    class FixingTest
    {
        const string TEST_REPOSITORY = "Test Repository";

        [OneTimeSetUp]
        public void CreateRepository()
        {
            bool exists = false;
            ILoggerRepository[] repositories = LogManager.GetAllRepositories();
            if (repositories != null)
            {
                foreach (ILoggerRepository r in repositories)
                {
                    if (r.Name == TEST_REPOSITORY)
                    {
                        exists = true;
                        break;
                    }
                }
            }
            if (!exists)
            {
                LogManager.CreateRepository(TEST_REPOSITORY);
            }

            // write-once
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "Log4Net Test thread";
            }
        }

        [Test]
        public void TestUnfixedValues()
        {
            LoggingEventData loggingEventData = BuildStandardEventData();

            // LoggingEvents occur at distinct points in time
            LoggingEvent loggingEvent = new LoggingEvent(
                loggingEventData.LocationInfo.GetType(),
                LogManager.GetRepository(TEST_REPOSITORY),
                loggingEventData.LoggerName,
                loggingEventData.Level,
                loggingEventData.Message,
                new Exception("This is the exception"));

            AssertExpectedLoggingEvent(loggingEvent, loggingEventData);

            Assert.AreEqual(FixFlags.None, loggingEvent.Fix, "Fixed Fields is incorrect");
        }

        [Test]
        public void TestAllFixedValues()
        {
            LoggingEventData loggingEventData = BuildStandardEventData();

            // LoggingEvents occur at distinct points in time
            LoggingEvent loggingEvent = new LoggingEvent(
                loggingEventData.LocationInfo.GetType(),
                LogManager.GetRepository(TEST_REPOSITORY),
                loggingEventData.LoggerName,
                loggingEventData.Level,
                loggingEventData.Message,
                new Exception("This is the exception"));

            AssertExpectedLoggingEvent(loggingEvent, loggingEventData);

            loggingEvent.Fix = FixFlags.All;

            Assert.AreEqual(FixFlags.LocationInfo | FixFlags.UserName | FixFlags.Identity | FixFlags.Partial | FixFlags.Message | FixFlags.ThreadName | FixFlags.Exception | FixFlags.Domain | FixFlags.Properties, loggingEvent.Fix, "Fixed Fields is incorrect");
        }

        [Test]
        public void TestNoFixedValues()
        {
            LoggingEventData loggingEventData = BuildStandardEventData();

            // LoggingEvents occur at distinct points in time
            LoggingEvent loggingEvent = new LoggingEvent(
                loggingEventData.LocationInfo.GetType(),
                LogManager.GetRepository(TEST_REPOSITORY),
                loggingEventData.LoggerName,
                loggingEventData.Level,
                loggingEventData.Message,
                new Exception("This is the exception"));

            AssertExpectedLoggingEvent(loggingEvent, loggingEventData);

            loggingEvent.Fix = FixFlags.None;

            Assert.AreEqual(FixFlags.None, loggingEvent.Fix, "Fixed Fields is incorrect");
        }

        private static LoggingEventData BuildStandardEventData()
        {
            LoggingEventData loggingEventData = new LoggingEventData();
            loggingEventData.LoggerName = typeof(FixingTest).FullName;
            loggingEventData.Level = Level.Warn;
            loggingEventData.Message = "Logging event works";
            loggingEventData.Domain = "ReallySimpleApp";
            loggingEventData.LocationInfo = new LocationInfo(typeof(FixingTest).Name, "Main", "Class1.cs", "29"); //Completely arbitary
            loggingEventData.ThreadName = Thread.CurrentThread.Name;
            loggingEventData.TimeStampUtc = DateTime.UtcNow.Date;
            loggingEventData.ExceptionString = "Exception occured here";
            loggingEventData.UserName = "TestUser";
            return loggingEventData;
        }

        private static void AssertExpectedLoggingEvent(LoggingEvent loggingEvent, LoggingEventData loggingEventData)
        {
            Assert.AreEqual("ReallySimpleApp", loggingEventData.Domain, "Domain is incorrect");
            Assert.AreEqual("System.Exception: This is the exception", loggingEvent.GetExceptionString(), "Exception is incorrect");
            Assert.AreEqual(null, loggingEventData.Identity, "Identity is incorrect");
            Assert.AreEqual(Level.Warn, loggingEventData.Level, "Level is incorrect");
            Assert.AreEqual("get_LocationInformation", loggingEvent.LocationInformation.MethodName, "Location Info is incorrect");
            Assert.AreEqual("Log4NetDemo.Test.Core.FixingTest", loggingEventData.LoggerName, "LoggerName is incorrect");
            Assert.AreEqual(LogManager.GetRepository(TEST_REPOSITORY), loggingEvent.Repository, "Repository is incorrect");
            Assert.AreEqual(Thread.CurrentThread.Name, loggingEventData.ThreadName, "ThreadName is incorrect");
            // This test is redundant as loggingEventData.TimeStamp is a value type and cannot be null
            // Assert.IsNotNull(loggingEventData.TimeStampUtc, "TimeStamp is incorrect");
            Assert.AreEqual("TestUser", loggingEventData.UserName, "UserName is incorrect");
            Assert.AreEqual("Logging event works", loggingEvent.RenderedMessage, "Message is incorrect");
        }

    }
}
