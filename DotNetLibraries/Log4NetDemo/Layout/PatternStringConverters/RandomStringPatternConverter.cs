using Log4NetDemo.Core.Interface;
using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class RandomStringPatternConverter : PatternConverter, IOptionHandler
    {
        private static readonly Random s_random = new Random();
        private int m_length = 4;

        public void ActivateOptions()
        {
            string optionStr = Option;
            if (optionStr != null && optionStr.Length > 0)
            {
                int lengthVal;
                if (SystemInfo.TryParse(optionStr, out lengthVal))
                {
                    m_length = lengthVal;
                }
                else
                {
                    LogLog.Error(declaringType, "RandomStringPatternConverter: Could not convert Option [" + optionStr + "] to Length Int32");
                }
            }
        }

        protected override void Convert(TextWriter writer, object state)
        {
            try
            {
                lock (s_random)
                {
                    for (int i = 0; i < m_length; i++)
                    {
                        int randValue = s_random.Next(36);

                        if (randValue < 26)
                        {
                            // Letter
                            char ch = (char)('A' + randValue);
                            writer.Write(ch);
                        }
                        else if (randValue < 36)
                        {
                            // Number
                            char ch = (char)('0' + (randValue - 26));
                            writer.Write(ch);
                        }
                        else
                        {
                            // Should not get here
                            writer.Write('X');
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "Error occurred while converting.", ex);
            }
        }

        private readonly static Type declaringType = typeof(RandomStringPatternConverter);
    }
}
