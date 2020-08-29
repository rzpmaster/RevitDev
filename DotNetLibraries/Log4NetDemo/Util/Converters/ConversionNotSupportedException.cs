using System;
using System.Runtime.Serialization;

namespace Log4NetDemo.Util.Converters
{
    public class ConversionNotSupportedException : ApplicationException
    {
        public ConversionNotSupportedException()
        {
        }

        public ConversionNotSupportedException(String message) : base(message)
        {
        }

        public ConversionNotSupportedException(String message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConversionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public static ConversionNotSupportedException Create(Type destinationType, object sourceValue)
        {
            return Create(destinationType, sourceValue, null);
        }

        public static ConversionNotSupportedException Create(Type destinationType, object sourceValue, Exception innerException)
        {
            if (sourceValue == null)
            {
                return new ConversionNotSupportedException("Cannot convert value [null] to type [" + destinationType + "]", innerException);
            }
            else
            {
                return new ConversionNotSupportedException("Cannot convert from type [" + sourceValue.GetType() + "] value [" + sourceValue + "] to type [" + destinationType + "]", innerException);
            }
        }
    }
}
