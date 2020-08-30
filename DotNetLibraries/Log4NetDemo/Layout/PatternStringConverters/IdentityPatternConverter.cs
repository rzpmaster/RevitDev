using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class IdentityPatternConverter : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            try
            {
                if (System.Threading.Thread.CurrentPrincipal != null &&
                    System.Threading.Thread.CurrentPrincipal.Identity != null &&
                    System.Threading.Thread.CurrentPrincipal.Identity.Name != null)
                {
                    writer.Write(System.Threading.Thread.CurrentPrincipal.Identity.Name);
                }
            }
            catch (System.Security.SecurityException)
            {
                // This security exception will occur if the caller does not have 
                // some undefined set of SecurityPermission flags.
                LogLog.Debug(declaringType, "Security exception while trying to get current thread principal. Error Ignored.");

                writer.Write(SystemInfo.NotAvailableText);
            }
        }

        private readonly static Type declaringType = typeof(IdentityPatternConverter);
    }
}
