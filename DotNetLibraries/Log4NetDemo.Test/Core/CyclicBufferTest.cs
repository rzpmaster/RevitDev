using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log4NetDemo.Appender;
using Log4NetDemo.Core.Data;
using NUnit.Framework;

namespace Log4NetDemo.Test.Core
{
    [TestFixture]
    class CyclicBufferTest
    {
        [Test]
        public void TestConstructorSize0()
        {
            CyclicBuffer cb = null;
            Assert.Throws<ArgumentOutOfRangeException>(() => cb = new CyclicBuffer(0));
        }

        [Test]
        public void TestSize1()
        {
            CyclicBuffer cb = new CyclicBuffer(1);

            Assert.AreEqual(0, cb.Length, "Empty Buffer should have length 0");
            Assert.AreEqual(1, cb.MaxSize, "Buffer should have max size 1");

            LoggingEvent event1 = new LoggingEvent(null, null, null, null, "event1", null);
            LoggingEvent event2 = new LoggingEvent(null, null, null, null, "event2", null);

            LoggingEvent discardedEvent = cb.Append(event1);

            Assert.IsNull(discardedEvent, "No event should be discarded untill the buffer is full");
            Assert.AreEqual(1, cb.Length, "Buffer should have length 1");
            Assert.AreEqual(1, cb.MaxSize, "Buffer should still have max size 1");


            discardedEvent = cb.Append(event2);

            Assert.AreSame(event1, discardedEvent, "Expect event1 to now be discarded");
            Assert.AreEqual(1, cb.Length, "Buffer should still have length 1");
            Assert.AreEqual(1, cb.MaxSize, "Buffer should really still have max size 1");

            LoggingEvent[] discardedEvents = cb.PopAll();

            Assert.AreEqual(1, discardedEvents.Length, "Poped events length should be 1");
            Assert.AreSame(event2, discardedEvents[0], "Expect event2 to now be popped");
            Assert.AreEqual(0, cb.Length, "Buffer should be back to length 0");
            Assert.AreEqual(1, cb.MaxSize, "Buffer should really really still have max size 1");
        }

        [Test]
        public void TestSize2()
        {
            CyclicBuffer cb = new CyclicBuffer(2);

            Assert.AreEqual(0, cb.Length, "Empty Buffer should have length 0");
            Assert.AreEqual(2, cb.MaxSize, "Buffer should have max size 2");

            LoggingEvent event1 = new LoggingEvent(null, null, null, null, "event1", null);
            LoggingEvent event2 = new LoggingEvent(null, null, null, null, "event2", null);
            LoggingEvent event3 = new LoggingEvent(null, null, null, null, "event3", null);

            LoggingEvent discardedEvent;

            discardedEvent = cb.Append(event1);
            Assert.IsNull(discardedEvent, "No event should be discarded after append 1");
            discardedEvent = cb.Append(event2);
            Assert.IsNull(discardedEvent, "No event should be discarded after append 2");

            discardedEvent = cb.Append(event3);
            Assert.AreSame(event1, discardedEvent, "Expect event1 to now be discarded");

            discardedEvent = cb.PopOldest();
            Assert.AreSame(event2, discardedEvent, "Expect event2 to now be discarded");

            LoggingEvent[] discardedEvents = cb.PopAll();

            Assert.AreEqual(1, discardedEvents.Length, "Poped events length should be 1");
            Assert.AreSame(event3, discardedEvents[0], "Expect event3 to now be popped");
            Assert.AreEqual(0, cb.Length, "Buffer should be back to length 0");
            Assert.AreEqual(2, cb.MaxSize, "Buffer should really really still have max size 2");
        }
    }
}
