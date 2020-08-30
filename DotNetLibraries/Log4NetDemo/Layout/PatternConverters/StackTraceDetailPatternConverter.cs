using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;
using System.Text;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal class StackTraceDetailPatternConverter : StackTracePatternConverter
    {
        internal override string GetMethodInformation(MethodItem method)
        {
            string returnValue = "";

            try
            {
                string param = "";
                string[] names = method.Parameters;
                StringBuilder sb = new StringBuilder();
                if (names != null && names.GetUpperBound(0) > 0)
                {
                    for (int i = 0; i <= names.GetUpperBound(0); i++)
                    {
                        sb.AppendFormat("{0}, ", names[i]);
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 2, 2);
                    param = sb.ToString();
                }

                returnValue = base.GetMethodInformation(method) + "(" + param + ")";
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "An exception ocurred while retreiving method information.", ex);
            }

            return returnValue;
        }

        private readonly static Type declaringType = typeof(StackTracePatternConverter);
    }
}
