using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleIocDemo.Attributes;

namespace SimpleIocDemo.Test.Stubs
{
    public class TestClass1 : ITestClass
    {
        public static int InstancesCount
        {
            get;
            private set;
        }

        public static void Reset()
        {
            InstancesCount = 0;
        }

        public TestClass1()
        {
            Identifier = Guid.NewGuid().ToString();
            InstancesCount++;
        }

        public string Identifier
        {
            get;
            private set;
        }
    }

    public class TestClass2
    {
    }

    public class TestClass3
    {
        public ITestClass SavedProperty
        {
            get;
            set;
        }

        public TestClass3(ITestClass parameter)
        {
            SavedProperty = parameter;
        }
    }

    public class TestClass4 : ITestClass
    {
    }

    public class TestClass5
    {
        public ITestClass MyProperty
        {
            get;
            private set;
        }

        public TestClass5()
        {

        }

        [PreferredConstructor]
        public TestClass5(ITestClass myProperty)
        {
            MyProperty = myProperty;
        }
    }

    public class TestClass6
    {
        public ITestClass MyProperty
        {
            get;
            set;
        }

        public TestClass6()
        {

        }

        public TestClass6(ITestClass myProperty)
        {
            MyProperty = myProperty;
        }
    }

    public class TestClass7
    {
        private TestClass7()
        {
        }

        static TestClass7()
        {

        }
    }

    public class TestClass8
    {
        static TestClass8()
        {

        }

        public TestClass8()
        {

        }
    }

    public class TestClass9
    {
        public TestClass9()
        {

        }

        internal TestClass9(string param)
        {

        }
    }
}
