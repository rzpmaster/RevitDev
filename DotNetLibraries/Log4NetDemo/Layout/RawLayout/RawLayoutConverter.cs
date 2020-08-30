using Log4NetDemo.Util.Converters;
using System;

namespace Log4NetDemo.Layout.RawLayout
{
    public class RawLayoutConverter : IConvertFrom
    {
        public bool CanConvertFrom(Type sourceType)
        {
            // Accept an ILayout object
            return (typeof(ILayout).IsAssignableFrom(sourceType));
        }

        public object ConvertFrom(object source)
        {
            ILayout layout = source as ILayout;
            if (layout != null)
            {
                return new Layout2RawLayoutAdapter(layout);
            }
            throw ConversionNotSupportedException.Create(typeof(IRawLayout), source);
        }
    }
}
