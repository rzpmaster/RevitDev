using Log4NetDemo.Repository;
using Log4NetDemo.Util.Converters;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Log4NetDemo.Util
{
    public sealed class OptionConverter
    {
        private OptionConverter() { }

        public static bool ToBoolean(string argValue, bool defaultValue)
        {
            if (argValue != null && argValue.Length > 0)
            {
                try
                {
                    return bool.Parse(argValue);
                }
                catch (Exception e)
                {
                    LogLog.Error(declaringType, "[" + argValue + "] is not in proper bool form.", e);
                }
            }
            return defaultValue;
        }

        public static long ToFileSize(string argValue, long defaultValue)
        {
            if (argValue == null)
            {
                return defaultValue;
            }

            string s = argValue.Trim().ToUpper(CultureInfo.InvariantCulture);
            long multiplier = 1;
            int index;

            if ((index = s.IndexOf("KB")) != -1)
            {
                multiplier = 1024;
                s = s.Substring(0, index);
            }
            else if ((index = s.IndexOf("MB")) != -1)
            {
                multiplier = 1024 * 1024;
                s = s.Substring(0, index);
            }
            else if ((index = s.IndexOf("GB")) != -1)
            {
                multiplier = 1024 * 1024 * 1024;
                s = s.Substring(0, index);
            }
            if (s != null)
            {
                // Try again to remove whitespace between the number and the size specifier
                s = s.Trim();

                long longVal;
                if (SystemInfo.TryParse(s, out longVal))
                {
                    return longVal * multiplier;
                }
                else
                {
                    LogLog.Error(declaringType, "OptionConverter: [" + s + "] is not in the correct file size syntax.");
                }
            }
            return defaultValue;
        }

        public static object ConvertStringTo(Type target, string txt)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            // If we want a string we already have the correct type
            if (typeof(string) == target || typeof(object) == target)
            {
                return txt;
            }

            // First lets try to find a type converter
            IConvertFrom typeConverter = ConverterRegistry.GetConvertFrom(target);
            if (typeConverter != null && typeConverter.CanConvertFrom(typeof(string)))
            {
                // Found appropriate converter
                return typeConverter.ConvertFrom(txt);
            }
            else
            {
                if (target.IsEnum)
                {
                    // Target type is an enum.

                    // Use the Enum.Parse(EnumType, string) method to get the enum value
                    return ParseEnum(target, txt, true);
                }
                else
                {
                    // We essentially make a guess that to convert from a string
                    // to an arbitrary type T there will be a static method defined on type T called Parse
                    // that will take an argument of type string. i.e. T.Parse(string)->T we call this
                    // method to convert the string to the type required by the property.
                    MethodInfo meth = target.GetMethod("Parse", new Type[] { typeof(string) });
                    if (meth != null)
                    {
                        // Call the Parse method

                        return meth.Invoke(null, BindingFlags.InvokeMethod, null, new object[] { txt }, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // No Parse() method found.
                    }
                }
            }
            return null;
        }

        public static bool CanConvertTypeTo(Type sourceType, Type targetType)
        {
            if (sourceType == null || targetType == null)
            {
                return false;
            }

            // Check if we can assign directly from the source type to the target type
            if (targetType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            // Look for a To converter
            IConvertTo tcSource = ConverterRegistry.GetConvertTo(sourceType, targetType);
            if (tcSource != null)
            {
                if (tcSource.CanConvertTo(targetType))
                {
                    return true;
                }
            }

            // Look for a From converter
            IConvertFrom tcTarget = ConverterRegistry.GetConvertFrom(targetType);
            if (tcTarget != null)
            {
                if (tcTarget.CanConvertFrom(sourceType))
                {
                    return true;
                }
            }

            return false;
        }

        public static object ConvertTypeTo(object sourceInstance, Type targetType)
        {
            Type sourceType = sourceInstance.GetType();

            // Check if we can assign directly from the source type to the target type
            if (targetType.IsAssignableFrom(sourceType))
            {
                return sourceInstance;
            }

            // Look for a TO converter
            IConvertTo tcSource = ConverterRegistry.GetConvertTo(sourceType, targetType);
            if (tcSource != null)
            {
                if (tcSource.CanConvertTo(targetType))
                {
                    return tcSource.ConvertTo(sourceInstance, targetType);
                }
            }

            // Look for a FROM converter
            IConvertFrom tcTarget = ConverterRegistry.GetConvertFrom(targetType);
            if (tcTarget != null)
            {
                if (tcTarget.CanConvertFrom(sourceType))
                {
                    return tcTarget.ConvertFrom(sourceInstance);
                }
            }

            throw new ArgumentException("Cannot convert source object [" + sourceInstance.ToString() + "] to target type [" + targetType.Name + "]", "sourceInstance");
        }

        /// <summary>
        /// 实例化一个对象
        /// </summary>
        /// <param name="className"></param>
        /// <param name="superClass"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static object InstantiateByClassName(string className, Type superClass, object defaultValue)
        {
            if (className != null)
            {
                try
                {
                    Type classObj = SystemInfo.GetTypeFromString(className, true, true);
                    if (!superClass.IsAssignableFrom(classObj))
                    {
                        LogLog.Error(declaringType, "OptionConverter: A [" + className + "] object is not assignable to a [" + superClass.FullName + "] variable.");
                        return defaultValue;
                    }
                    return Activator.CreateInstance(classObj);
                }
                catch (Exception e)
                {
                    LogLog.Error(declaringType, "Could not instantiate class [" + className + "].", e);
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// string 变量替换
        /// </summary>
        /// <param name="value">执行变量替换的字符串</param>
        /// <param name="props"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>替换分割符 <b>${</b> and <b>}</b></para>
        /// </remarks>
        public static string SubstituteVariables(string value, System.Collections.IDictionary props)
        {
            StringBuilder buf = new StringBuilder();

            int i = 0;
            int j, k;

            while (true)
            {
                j = value.IndexOf(DELIM_START, i);
                if (j == -1)
                {
                    if (i == 0)
                    {
                        return value;
                    }
                    else
                    {
                        buf.Append(value.Substring(i, value.Length - i));
                        return buf.ToString();
                    }
                }
                else
                {
                    buf.Append(value.Substring(i, j - i));
                    k = value.IndexOf(DELIM_STOP, j);
                    if (k == -1)
                    {
                        throw new LogException("[" + value + "] has no closing brace. Opening brace at position [" + j + "]");
                    }
                    else
                    {
                        j += DELIM_START_LEN;
                        string key = value.Substring(j, k - j);

                        string replacement = props[key] as string;

                        if (replacement != null)
                        {
                            buf.Append(replacement);
                        }
                        i = k + DELIM_STOP_LEN;
                    }
                }
            }
        }

        #region Private Static Methods

        private static object ParseEnum(System.Type enumType, string value, bool ignoreCase)
        {
            return Enum.Parse(enumType, value, ignoreCase);
        }

        #endregion

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the OptionConverter class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(OptionConverter);

        private const string DELIM_START = "${";
        private const char DELIM_STOP = '}';
        private const int DELIM_START_LEN = 2;
        private const int DELIM_STOP_LEN = 1;

        #endregion Private Static Fields
    }
}
