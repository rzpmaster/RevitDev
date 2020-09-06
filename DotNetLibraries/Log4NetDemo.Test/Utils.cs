using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log4NetDemo.Context;
using Log4NetDemo.Core;
using Log4NetDemo.Repository;

namespace Log4NetDemo.Test
{
    class Utils
    {
        internal const string PROPERTY_KEY = "prop1";

        private Utils()
        {
        }

        internal static void RemovePropertyFromAllContexts()
        {
            GlobalContext.Properties.Remove(PROPERTY_KEY);
            ThreadContext.Properties.Remove(PROPERTY_KEY);
        }

        internal static ILoggerRepository GetRepository()
        {
            return LogManager.GetRepository();
        }

        internal static ILog GetLogger(string name)
        {
            return LogManager.GetLogger(name);
        }
    }
}
