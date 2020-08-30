using Log4NetDemo.Context;
using Log4NetDemo.Util.Collections;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class PropertyPatternConverter : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            CompositeProperties compositeProperties = new CompositeProperties();

            PropertiesDictionary logicalThreadProperties = LogicalThreadContext.Properties.GetProperties(false);
            if (logicalThreadProperties != null)
            {
                compositeProperties.Add(logicalThreadProperties);
            }

            PropertiesDictionary threadProperties = ThreadContext.Properties.GetProperties(false);
            if (threadProperties != null)
            {
                compositeProperties.Add(threadProperties);
            }

            // TODO: Add Repository Properties
            compositeProperties.Add(GlobalContext.Properties.GetReadOnlyProperties());

            if (Option != null)
            {
                // Write the value for the specified key
                WriteObject(writer, null, compositeProperties[Option]);
            }
            else
            {
                // Write all the key value pairs
                WriteDictionary(writer, null, compositeProperties.Flatten());
            }
        }
    }
}
