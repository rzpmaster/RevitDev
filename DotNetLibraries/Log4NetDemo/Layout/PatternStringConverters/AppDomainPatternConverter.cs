using Log4NetDemo.Util;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class AppDomainPatternConverter : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            writer.Write(SystemInfo.ApplicationFriendlyName);
        }
    }
}
