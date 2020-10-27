using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Log4NetDemo.Util.Collections;
using NUnit.Framework;

namespace Log4NetDemo.Test.Util
{
    [TestFixture]
    class PropertiesDictionaryTest
    {
        [Test]
        public void TestSerialization()
        {
            PropertiesDictionary pd = new PropertiesDictionary();

            for (int i = 0; i < 10; i++)
            {
                pd[i.ToString()] = i;
            }

            Assert.AreEqual(10, pd.Count, "Dictionary should have 10 items");

            // Serialize the properties into a memory stream
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memory = new MemoryStream();
            formatter.Serialize(memory, pd);

            // Deserialize the stream into a new properties dictionary
            memory.Position = 0;
            PropertiesDictionary pd2 = (PropertiesDictionary)formatter.Deserialize(memory);

            Assert.AreEqual(10, pd2.Count, "Deserialized Dictionary should have 10 items");

            foreach (string key in pd.GetKeys())
            {
                Assert.AreEqual(pd[key], pd2[key], "Check Value Persisted for key [{0}]", key);
            }
        }
    }
}
