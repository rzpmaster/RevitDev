using Log4NetDemo.Util.Converters.TypeConverters;
using System;
using System.Collections;
using System.Text;

namespace Log4NetDemo.Util.Converters
{
    public sealed class ConverterRegistry
    {
        private ConverterRegistry()
        {
        }

        static ConverterRegistry()
        {
            // Add predefined converters here
            AddConverter(typeof(bool), typeof(BooleanConverter));
            AddConverter(typeof(Encoding), typeof(EncodingConverter));
            AddConverter(typeof(Type), typeof(TypeConverter));
            //AddConverter(typeof(Layout.PatternLayout), typeof(PatternLayoutConverter));
            //AddConverter(typeof(Util.PatternString), typeof(PatternStringConverter));
            AddConverter(typeof(System.Net.IPAddress), typeof(IPAddressConverter));
        }

        public static void AddConverter(Type destinationType, object converter)
        {
            if (destinationType != null && converter != null)
            {
                lock (s_type2converter)
                {
                    s_type2converter[destinationType] = converter;
                }
            }
        }

        public static void AddConverter(Type destinationType, Type converterType)
        {
            AddConverter(destinationType, CreateConverterInstance(converterType));
        }

        public static IConvertTo GetConvertTo(Type sourceType, Type destinationType)
        {
            // TODO: Support inheriting type converters.
            // i.e. getting a type converter for a base of sourceType

            // TODO: Is destinationType required? We don't use it for anything.

            lock (s_type2converter)
            {
                // Lookup in the static registry
                IConvertTo converter = s_type2converter[sourceType] as IConvertTo;

                if (converter == null)
                {
                    // Lookup using attributes
                    converter = GetConverterFromAttribute(sourceType) as IConvertTo;

                    if (converter != null)
                    {
                        // Store in registry
                        s_type2converter[sourceType] = converter;
                    }
                }

                return converter;
            }
        }

        public static IConvertFrom GetConvertFrom(Type destinationType)
        {
            // TODO: Support inheriting type converters.
            // i.e. getting a type converter for a base of destinationType

            lock (s_type2converter)
            {
                // Lookup in the static registry
                IConvertFrom converter = s_type2converter[destinationType] as IConvertFrom;

                if (converter == null)
                {
                    // Lookup using attributes
                    converter = GetConverterFromAttribute(destinationType) as IConvertFrom;

                    if (converter != null)
                    {
                        // Store in registry
                        s_type2converter[destinationType] = converter;
                    }
                }

                return converter;
            }
        }

        private static object GetConverterFromAttribute(Type destinationType)
        {
            // Look for an attribute on the destination type
            object[] attributes = destinationType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
            if (attributes != null && attributes.Length > 0)
            {
                TypeConverterAttribute tcAttr = attributes[0] as TypeConverterAttribute;
                if (tcAttr != null)
                {
                    Type converterType = SystemInfo.GetTypeFromString(destinationType, tcAttr.ConverterTypeName, false, true);
                    return CreateConverterInstance(converterType);
                }
            }

            // Not found converter using attributes
            return null;
        }

        private static object CreateConverterInstance(Type converterType)
        {
            if (converterType == null)
            {
                throw new ArgumentNullException("converterType", "CreateConverterInstance cannot create instance, converterType is null");
            }

            // Check type is a converter
            if (typeof(IConvertFrom).IsAssignableFrom(converterType) || typeof(IConvertTo).IsAssignableFrom(converterType))
            {
                try
                {
                    // Create the type converter
                    return Activator.CreateInstance(converterType);
                }
                catch (Exception ex)
                {
                    LogLog.Error(declaringType, "Cannot CreateConverterInstance of type [" + converterType.FullName + "], Exception in call to Activator.CreateInstance", ex);
                }
            }
            else
            {
                LogLog.Error(declaringType, "Cannot CreateConverterInstance of type [" + converterType.FullName + "], type does not implement IConvertFrom or IConvertTo");
            }
            return null;
        }

        /// <summary>
        /// Mapping from <see cref="Type" /> to type converter.
        /// </summary>
        private static Hashtable s_type2converter = new Hashtable();
        private readonly static Type declaringType = typeof(ConverterRegistry);
    }
}
