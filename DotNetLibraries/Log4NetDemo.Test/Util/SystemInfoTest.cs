using Log4NetDemo.Util;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Log4NetDemo.Test.Util
{
    [TestFixture]
    class SystemInfoTest
    {
        [Test]
        public void TestAssemblyLocationInfoDoesNotThrowNotSupportedExceptionForDynamicAssembly()
        {
            var systemInfoAssemblyLocationMethod = GetAssemblyLocationInfoMethodCall();

            Assert.DoesNotThrow(() => systemInfoAssemblyLocationMethod());
        }

        private static Func<string> GetAssemblyLocationInfoMethodCall()
        {
            var method = typeof(SystemInfoTest).GetMethod("TestAssemblyLocationInfoMethod", new Type[0]);
            var methodCall = Expression.Call(null, method, new Expression[0]);
            return Expression.Lambda<Func<string>>(methodCall, new ParameterExpression[0]).Compile();
        }

        [Test]
        public void TestAssemblyLocationInfoMethod()
        {
            var location = SystemInfo.AssemblyLocationInfo(Assembly.GetCallingAssembly());
            Assert.That(location, Is.EqualTo(42), "Some useful error message");
        }

        [Test]
        public void TestGetTypeFromStringFullyQualified()
        {
            Type t;

            t = GetTypeFromString("log4net.Tests.Util.SystemInfoTest,log4net.Tests", false, false);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case sensitive type load");

            t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST,log4net.Tests", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4net.tests.util.systeminfotest,log4net.Tests", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        [Platform(Include = "Win")]
        public void TestGetTypeFromStringCaseInsensitiveOnAssemblyName()
        {
            Type t;

            t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST,LOG4NET.TESTS", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4net.tests.util.systeminfotest,log4net.tests", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        public void TestGetTypeFromStringRelative()
        {
            Type t;

            t = GetTypeFromString("log4net.Tests.Util.SystemInfoTest", false, false);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case sensitive type load");

            t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4net.tests.util.systeminfotest", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        public void TestGetTypeFromStringSearch()
        {
            Type t;

            t = GetTypeFromString("log4net.Util.SystemInfo", false, false);
            Assert.AreSame(typeof(SystemInfo), t,
                                       string.Format("Test explicit case sensitive type load found {0} rather than {1}",
                                                     t.AssemblyQualifiedName, typeof(SystemInfo).AssemblyQualifiedName));

            t = GetTypeFromString("LOG4NET.UTIL.SYSTEMINFO", false, true);
            Assert.AreSame(typeof(SystemInfo), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4net.util.systeminfo", false, true);
            Assert.AreSame(typeof(SystemInfo), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        public void TestGetTypeFromStringFails1()
        {
            Type t;

            t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST,LOG4NET.TESTS", false, false);
            Assert.AreSame(null, t, "Test explicit case sensitive fails type load");

            //t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST,LOG4NET.TESTS", true, false);
            Assert.Throws<TypeLoadException>(() => GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST,LOG4NET.TESTS", true, false));
        }

        [Test]
        public void TestGetTypeFromStringFails2()
        {
            Type t;

            t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST", false, false);
            Assert.AreSame(null, t, "Test explicit case sensitive fails type load");

            //t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST", true, false);
            Assert.Throws<TypeLoadException>(() => GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST", true, false));
        }

        private Type GetTypeFromString(string typeName, bool throwOnError, bool ignoreCase)
        {
            return SystemInfo.GetTypeFromString(typeName, throwOnError, ignoreCase);
        }

        [Test]
        public void EqualsIgnoringCase_BothNull_true()
        {
            Assert.True(SystemInfo.EqualsIgnoringCase(null, null));
        }

        [Test]
        public void EqualsIgnoringCase_LeftNull_false()
        {
            Assert.False(SystemInfo.EqualsIgnoringCase(null, "foo"));
        }

        [Test]
        public void EqualsIgnoringCase_RightNull_false()
        {
            Assert.False(SystemInfo.EqualsIgnoringCase("foo", null));
        }

        [Test]
        public void EqualsIgnoringCase_SameStringsSameCase_true()
        {
            Assert.True(SystemInfo.EqualsIgnoringCase("foo", "foo"));
        }

        [Test]
        public void EqualsIgnoringCase_SameStringsDifferentCase_true()
        {
            Assert.True(SystemInfo.EqualsIgnoringCase("foo", "FOO"));
        }

        [Test]
        public void EqualsIgnoringCase_DifferentStrings_false()
        {
            Assert.False(SystemInfo.EqualsIgnoringCase("foo", "foobar"));
        }
    }
}
