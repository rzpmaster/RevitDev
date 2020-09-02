using System;
using System.Linq.Expressions;
using System.Reflection;
using Log4NetDemo.Util;
using NUnit.Framework;

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

        public static string TestAssemblyLocationInfoMethod()
        {
            return SystemInfo.AssemblyLocationInfo(Assembly.GetCallingAssembly());
        }

        [Test]
        public void TestGetTypeFromStringFullyQualified()
        {
            Type t;

            t = GetTypeFromString("Log4NetDemo.Test.Util.SystemInfoTest,Log4NetDemo.Test", false, false);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case sensitive type load");

            t = GetTypeFromString("LOG4NETDEMO.TEST.UTIL.SYSTEMINFOTEST,LOG4NETDEMO.TEST", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4netdemo.test.util.systeminfotest,Log4NetDemo.Test", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        [Platform(Include = "Win")]
        public void TestGetTypeFromStringCaseInsensitiveOnAssemblyName()
        {
            Type t;

            t = GetTypeFromString("LOG4NETDEMO.TEST.UTIL.SYSTEMINFOTEST,LOG4NETDEMO.TEST", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4netdemo.test.util.systeminfotest,log4netdemo.test", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        public void TestGetTypeFromStringRelative()
        {
            Type t;

            t = GetTypeFromString("Log4NetDemo.Test.Util.SystemInfoTest", false, false);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case sensitive type load");

            t = GetTypeFromString("LOG4NETDEMO.TEST.UTIL.SYSTEMINFOTEST", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4netdemo.test.util.systeminfotest", false, true);
            Assert.AreSame(typeof(SystemInfoTest), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        public void TestGetTypeFromStringSearch()
        {
            Type t;

            t = GetTypeFromString("Log4NetDemo.Util.SystemInfo", false, false);
            Assert.AreSame(typeof(SystemInfo), t,
                                       string.Format("Test explicit case sensitive type load found {0} rather than {1}",
                                                     t.AssemblyQualifiedName, typeof(SystemInfo).AssemblyQualifiedName));

            t = GetTypeFromString("LOG4NETDEMO.UTIL.SYSTEMINFO", false, true);
            Assert.AreSame(typeof(SystemInfo), t, "Test explicit case in-sensitive type load caps");

            t = GetTypeFromString("log4netdemo.util.systeminfo", false, true);
            Assert.AreSame(typeof(SystemInfo), t, "Test explicit case in-sensitive type load lower");
        }

        [Test]
        public void TestGetTypeFromStringFails1()
        {
            Type t;

            t = GetTypeFromString("LOG4NETDEMO.TEST.UTIL.SYSTEMINFOTEST,LOG4NETDEMO.TEST", false, false);
            Assert.AreSame(null, t, "Test explicit case sensitive fails type load");

            //t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST,LOG4NET.TESTS", true, false);
            Assert.Throws<TypeLoadException>(() => GetTypeFromString("LOG4NETDEMO.TEST.UTIL.SYSTEMINFOTEST,LOG4NETDEMO.TEST", true, false));
        }

        [Test]
        public void TestGetTypeFromStringFails2()
        {
            Type t;

            t = GetTypeFromString("LOG4NETDEMO.TEST.UTIL.SYSTEMINFOTEST", false, false);
            Assert.AreSame(null, t, "Test explicit case sensitive fails type load");

            //t = GetTypeFromString("LOG4NET.TESTS.UTIL.SYSTEMINFOTEST", true, false);
            Assert.Throws<TypeLoadException>(() => GetTypeFromString("LOG4NETDEMO.TEST.UTIL.SYSTEMINFOTEST", true, false));
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
