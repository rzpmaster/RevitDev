using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Log4NetDemo
{
    class Program
    {
        //static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static readonly log4net.ILog log = LogHelper.GetLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("log4net demo");

            log.Error("this is my error message.");

            Console.ReadKey();
        }
    }
}
