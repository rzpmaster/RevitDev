using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class EnvironmentPatternConverter : PatternConverter
    {
        override protected void Convert(TextWriter writer, object state)
        {
            try
            {
                if (this.Option != null && this.Option.Length > 0)
                {
                    // Lookup the environment variable
                    string envValue = Environment.GetEnvironmentVariable(this.Option);

                    // If we didn't see it for the process, try a user level variable.
                    if (envValue == null)
                    {
                        envValue = Environment.GetEnvironmentVariable(this.Option, EnvironmentVariableTarget.User);
                    }

                    // If we still didn't find it, try a system level one.
                    if (envValue == null)
                    {
                        envValue = Environment.GetEnvironmentVariable(this.Option, EnvironmentVariableTarget.Machine);
                    }
                    if (envValue != null && envValue.Length > 0)
                    {
                        writer.Write(envValue);
                    }
                }
            }
            catch (System.Security.SecurityException secEx)
            {
                // This security exception will occur if the caller does not have 
                // unrestricted environment permission. If this occurs the expansion 
                // will be skipped with the following warning message.
                LogLog.Debug(declaringType, "Security exception while trying to expand environment variables. Error Ignored. No Expansion.", secEx);
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Error occurred while converting environment variable.", ex);
            }
        }

        private readonly static Type declaringType = typeof(EnvironmentPatternConverter);
    }
}
