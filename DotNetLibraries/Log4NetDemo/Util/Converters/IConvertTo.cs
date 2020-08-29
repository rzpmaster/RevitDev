using System;

namespace Log4NetDemo.Util.Converters
{
    public interface IConvertTo
    {
        bool CanConvertTo(Type targetType);
        object ConvertTo(object source, Type targetType);
    }
}
