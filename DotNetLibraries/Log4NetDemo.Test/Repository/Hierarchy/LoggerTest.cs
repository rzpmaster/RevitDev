using System.Collections;
using Log4NetDemo.Core;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Repository.Hierarchy;
using Log4NetDemo.Test.Appender.TestAppender;
using NUnit.Framework;

namespace Log4NetDemo.Test.Repository.Hierarchy
{
    [TestFixture]
    class LoggerTest
    {
        private Logger log;

        // A short message.
        private static string MSG = "M";

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            // Regular users should not use the clear method lightly!
            Utils.GetRepository().ResetConfiguration();
            Utils.GetRepository().Shutdown();
            ((Log4NetDemo.Repository.Hierarchy.Hierarchy)Utils.GetRepository()).Clear();
        }

        [Test]
        public void TestAppender1()
        {
            log = (Logger)Utils.GetLogger("test").Logger;
            CountingAppender a1 = new CountingAppender();
            a1.Name = "testAppender1";
            log.AddAppender(a1);

            IEnumerator enumAppenders = ((IEnumerable)log.Appenders).GetEnumerator();
            Assert.IsTrue(enumAppenders.MoveNext());
            CountingAppender aHat = (CountingAppender)enumAppenders.Current;
            Assert.AreEqual(a1, aHat);
        }

        [Test]
        public void TestAppender2()
        {
            CountingAppender a1 = new CountingAppender();
            a1.Name = "testAppender2.1";
            CountingAppender a2 = new CountingAppender();
            a2.Name = "testAppender2.2";

            log = (Logger)Utils.GetLogger("test").Logger;
            log.AddAppender(a1);
            log.AddAppender(a2);

            CountingAppender aHat = (CountingAppender)log.GetAppender(a1.Name);
            Assert.AreEqual(a1, aHat);

            aHat = (CountingAppender)log.GetAppender(a2.Name);
            Assert.AreEqual(a2, aHat);

            log.RemoveAppender("testAppender2.1");

            IEnumerator enumAppenders = ((IEnumerable)log.Appenders).GetEnumerator();
            Assert.IsTrue(enumAppenders.MoveNext());
            aHat = (CountingAppender)enumAppenders.Current;
            Assert.AreEqual(a2, aHat);
            Assert.IsTrue(!enumAppenders.MoveNext());

            aHat = (CountingAppender)log.GetAppender(a2.Name);
            Assert.AreEqual(a2, aHat);
        }

        [Test]
        public void TestAdditivity1()
        {
            Logger a = (Logger)Utils.GetLogger("a").Logger;
            Logger ab = (Logger)Utils.GetLogger("a.b").Logger;
            CountingAppender ca = new CountingAppender();

            a.AddAppender(ca);
            a.Repository.Configured = true;

            Assert.AreEqual(ca.Counter, 0);
            ab.Log(Level.Debug, MSG, null);
            Assert.AreEqual(ca.Counter, 1);
            ab.Log(Level.Info, MSG, null);
            Assert.AreEqual(ca.Counter, 2);
            ab.Log(Level.Warn, MSG, null);
            Assert.AreEqual(ca.Counter, 3);
            ab.Log(Level.Error, MSG, null);
            Assert.AreEqual(ca.Counter, 4);
        }

        [Test]
        public void TestAdditivity2()
        {
            Logger a = (Logger)Utils.GetLogger("a").Logger;
            Logger ab = (Logger)Utils.GetLogger("a.b").Logger;
            Logger abc = (Logger)Utils.GetLogger("a.b.c").Logger;
            Logger x = (Logger)Utils.GetLogger("x").Logger;

            CountingAppender ca1 = new CountingAppender();
            CountingAppender ca2 = new CountingAppender();

            a.AddAppender(ca1);
            abc.AddAppender(ca2);
            a.Repository.Configured = true;

            Assert.AreEqual(ca1.Counter, 0);
            Assert.AreEqual(ca2.Counter, 0);

            ab.Log(Level.Debug, MSG, null);
            Assert.AreEqual(ca1.Counter, 1);
            Assert.AreEqual(ca2.Counter, 0);

            abc.Log(Level.Debug, MSG, null);
            Assert.AreEqual(ca1.Counter, 2);
            Assert.AreEqual(ca2.Counter, 1);

            x.Log(Level.Debug, MSG, null);
            Assert.AreEqual(ca1.Counter, 2);
            Assert.AreEqual(ca2.Counter, 1);
        }

        [Test]
        public void TestAdditivity3()
        {
            Logger root = ((Log4NetDemo.Repository.Hierarchy.Hierarchy)Utils.GetRepository()).Root;
            Logger a = (Logger)Utils.GetLogger("a").Logger;
            Logger ab = (Logger)Utils.GetLogger("a.b").Logger;
            Logger abc = (Logger)Utils.GetLogger("a.b.c").Logger;

            CountingAppender caRoot = new CountingAppender();
            CountingAppender caA = new CountingAppender();
            CountingAppender caABC = new CountingAppender();

            root.AddAppender(caRoot);
            a.AddAppender(caA);
            abc.AddAppender(caABC);
            a.Repository.Configured = true;

            Assert.AreEqual(caRoot.Counter, 0);
            Assert.AreEqual(caA.Counter, 0);
            Assert.AreEqual(caABC.Counter, 0);

            ab.Additivity = false;

            a.Log(Level.Debug, MSG, null);
            Assert.AreEqual(caRoot.Counter, 1);
            Assert.AreEqual(caA.Counter, 1);
            Assert.AreEqual(caABC.Counter, 0);

            ab.Log(Level.Debug, MSG, null);
            Assert.AreEqual(caRoot.Counter, 1);
            Assert.AreEqual(caA.Counter, 1);
            Assert.AreEqual(caABC.Counter, 0);

            abc.Log(Level.Debug, MSG, null);
            Assert.AreEqual(caRoot.Counter, 1);
            Assert.AreEqual(caA.Counter, 1);
            Assert.AreEqual(caABC.Counter, 1);
        }

        [Test]
        public void TestDisable1()
        {
            CountingAppender caRoot = new CountingAppender();
            Logger root = ((Log4NetDemo.Repository.Hierarchy.Hierarchy)Utils.GetRepository()).Root;
            root.AddAppender(caRoot);

            Log4NetDemo.Repository.Hierarchy.Hierarchy h = ((Log4NetDemo.Repository.Hierarchy.Hierarchy)Utils.GetRepository());
            h.Threshold = Level.Info;
            h.Configured = true;

            Assert.AreEqual(caRoot.Counter, 0);

            root.Log(Level.Debug, MSG, null);
            Assert.AreEqual(caRoot.Counter, 0);
            root.Log(Level.Info, MSG, null);
            Assert.AreEqual(caRoot.Counter, 1);
            root.Log(Level.Warn, MSG, null);
            Assert.AreEqual(caRoot.Counter, 2);
            root.Log(Level.Warn, MSG, null);
            Assert.AreEqual(caRoot.Counter, 3);

            h.Threshold = Level.Warn;
            root.Log(Level.Debug, MSG, null);
            Assert.AreEqual(caRoot.Counter, 3);
            root.Log(Level.Info, MSG, null);
            Assert.AreEqual(caRoot.Counter, 3);
            root.Log(Level.Warn, MSG, null);
            Assert.AreEqual(caRoot.Counter, 4);
            root.Log(Level.Error, MSG, null);
            Assert.AreEqual(caRoot.Counter, 5);
            root.Log(Level.Error, MSG, null);
            Assert.AreEqual(caRoot.Counter, 6);

            h.Threshold = Level.Off;
            root.Log(Level.Debug, MSG, null);
            Assert.AreEqual(caRoot.Counter, 6);
            root.Log(Level.Info, MSG, null);
            Assert.AreEqual(caRoot.Counter, 6);
            root.Log(Level.Warn, MSG, null);
            Assert.AreEqual(caRoot.Counter, 6);
            root.Log(Level.Error, MSG, null);
            Assert.AreEqual(caRoot.Counter, 6);
            root.Log(Level.Fatal, MSG, null);
            Assert.AreEqual(caRoot.Counter, 6);
            root.Log(Level.Fatal, MSG, null);
            Assert.AreEqual(caRoot.Counter, 6);
        }

        [Test]
        public void TestExists()
        {
            object a = Utils.GetLogger("a");
            object a_b = Utils.GetLogger("a.b");
            object a_b_c = Utils.GetLogger("a.b.c");

            object t;
            t = LogManager.Exists("xx");
            Assert.IsNull(t);
            t = LogManager.Exists("a");
            Assert.AreSame(a, t);
            t = LogManager.Exists("a.b");
            Assert.AreSame(a_b, t);
            t = LogManager.Exists("a.b.c");
            Assert.AreSame(a_b_c, t);
        }

        [Test]
        public void TestHierarchy1()
        {
            Log4NetDemo.Repository.Hierarchy.Hierarchy h = new Log4NetDemo.Repository.Hierarchy.Hierarchy();
            h.Root.Level = Level.Error;

            Logger a0 = (Logger)h.GetLogger("a");
            Assert.AreEqual("a", a0.Name);
            Assert.IsNull(a0.Level);
            Assert.AreSame(Level.Error, a0.EffectiveLevel);

            Logger a1 = (Logger)h.GetLogger("a");
            Assert.AreSame(a0, a1);
        }

    }
}
