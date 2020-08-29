using System;

namespace Log4NetDemo.Util.Converters
{
    public interface IConvertFrom
    {
        bool CanConvertFrom(Type sourceType);
        object ConvertFrom(object source);
    }
}
