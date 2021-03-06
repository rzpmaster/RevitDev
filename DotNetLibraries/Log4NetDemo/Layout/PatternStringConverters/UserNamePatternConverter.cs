﻿using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class UserNamePatternConverter : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            try
            {
                System.Security.Principal.WindowsIdentity windowsIdentity = null;
                windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
                if (windowsIdentity != null && windowsIdentity.Name != null)
                {
                    writer.Write(windowsIdentity.Name);
                }
            }
            catch (System.Security.SecurityException)
            {
                // This security exception will occur if the caller does not have 
                // some undefined set of SecurityPermission flags.
                LogLog.Debug(declaringType, "Security exception while trying to get current windows identity. Error Ignored.");

                writer.Write(SystemInfo.NotAvailableText);
            }
        }

        private readonly static Type declaringType = typeof(UserNamePatternConverter);
    }
}
