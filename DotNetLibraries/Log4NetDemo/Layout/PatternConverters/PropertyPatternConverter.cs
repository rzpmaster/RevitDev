using Log4NetDemo.Core.Data;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal sealed class PropertyPatternConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
			if (Option != null)
			{
				// Write the value for the specified key
				WriteObject(writer, loggingEvent.Repository, loggingEvent.LookupProperty(Option));
			}
			else
			{
				// Write all the key value pairs
				WriteDictionary(writer, loggingEvent.Repository, loggingEvent.GetProperties());
			}
		}
    }
}
