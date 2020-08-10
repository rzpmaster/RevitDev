using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo
{
    public class LogHelper
    {
        public static ILog GetLogger([CallerFilePath] string filename = "")
        {
            filename = Path.GetFileName(filename);
            return LogManager.GetLogger(filename);
        }
    }
}
