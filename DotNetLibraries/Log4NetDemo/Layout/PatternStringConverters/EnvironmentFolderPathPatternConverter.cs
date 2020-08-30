using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class EnvironmentFolderPathPatternConverter : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            try
            {
                if (Option != null && Option.Length > 0)
                {
                    Environment.SpecialFolder specialFolder =
                        (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), Option, true);

                    string envFolderPathValue = Environment.GetFolderPath(specialFolder);
                    if (envFolderPathValue != null && envFolderPathValue.Length > 0)
                    {
                        writer.Write(envFolderPathValue);
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

        private readonly static Type declaringType = typeof(EnvironmentFolderPathPatternConverter);
    }
}
