using Log4NetDemo.Core.Data;
using Log4NetDemo.Util.Converters.TypeConverters;

namespace Log4NetDemo.Layout.RawLayout
{
    [TypeConverter(typeof(RawLayoutConverter))]
    public interface IRawLayout
    {
        object Format(LoggingEvent loggingEvent);
    }
}
