using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class ProcessIdPatternConverter : PatternConverter
    {
        [System.Security.SecuritySafeCritical]
        protected override void Convert(TextWriter writer, object state)
        {
            try
            {
                writer.Write(System.Diagnostics.Process.GetCurrentProcess().Id);
            }
            catch (System.Security.SecurityException)
            {
                // This security exception will occur if the caller does not have 
                // some undefined set of SecurityPermission flags.
                LogLog.Debug(declaringType, "Security exception while trying to get current process id. Error Ignored.");

                writer.Write(SystemInfo.NotAvailableText);
            }
        }

        private readonly static Type declaringType = typeof(ProcessIdPatternConverter);
    }
}
