using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleIocDemo.Exceptions;
using SimpleIocDemo.Test.Stubs;

namespace SimpleIocDemo.Test
{
    [TestFixture]
    public class MultipleOrPrivateConstructorsTest
    {
        [Test]
        public void TestBuildInstanceWithMultipleConstructorsNotMarkedWithAttribute()
        {
            var property = new TestClass1();

            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register(() => new TestClass6(property));

            var instance1 = new TestClass6();
            Assert.IsNotNull(instance1);
            Assert.IsNull(instance1.MyProperty);

            var instance2 = SimpleIoc.Default.GetInstance<TestClass6>();
            Assert.IsNotNull(instance2);
            Assert.IsNotNull(instance2.MyProperty);
            Assert.AreSame(property, instance2.MyProperty);
        }

        [Test]
        public void TestBuildWithMultipleConstructors()
        {
            var property = new TestClass1();

            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register<ITestClass>(() => property);
            SimpleIoc.Default.Register<TestClass5>();

            var instance1 = new TestClass5();
            Assert.IsNotNull(instance1);
            Assert.IsNull(instance1.MyProperty);

            var instance2 = SimpleIoc.Default.GetInstance<TestClass5>();
            Assert.IsNotNull(instance2);
            Assert.IsNotNull(instance2.MyProperty);
            Assert.AreSame(property, instance2.MyProperty);
        }

        [Test]
        public void TestBuildWithMultipleConstructorsNotMarkedWithAttribute()
        {
            var property = new TestClass1();

            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register<ITestClass>(() => property);

            try
            {
                SimpleIoc.Default.Register<TestClass6>();
                Assert.Fail("ActivationException was expected");
            }
            catch (ActivationException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [Test]
        public void TestBuildWithPrivateConstructor()
        {
            SimpleIoc.Default.Reset();

            try
            {
                SimpleIoc.Default.Register<TestClass7>();
                Assert.Fail("ActivationException was expected");
            }
            catch (ActivationException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [Test]
        public void TestBuildWithStaticConstructor()
        {
            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register<TestClass8>();
        }

        [Test]
        public void TestPublicAndInternalConstructor()
        {
            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register<TestClass9>();
        }
    }
}
