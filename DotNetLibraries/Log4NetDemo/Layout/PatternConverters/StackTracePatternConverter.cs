using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using Log4NetDemo.Util;
using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal class StackTracePatternConverter : PatternLayoutConverter, IOptionHandler
    {
        private int m_stackFrameLevel = 1;

        public void ActivateOptions()
        {
            if (Option == null)
                return;

            string optStr = Option.Trim();
            if (optStr.Length != 0)
            {
                int stackLevelVal;
                if (SystemInfo.TryParse(optStr, out stackLevelVal))
                {
                    if (stackLevelVal <= 0)
                    {
                        LogLog.Error(declaringType, "StackTracePatternConverter: StackeFrameLevel option (" + optStr + ") isn't a positive integer.");
                    }
                    else
                    {
                        m_stackFrameLevel = stackLevelVal;
                    }
                }
                else
                {
                    LogLog.Error(declaringType, "StackTracePatternConverter: StackFrameLevel option \"" + optStr + "\" not a decimal integer.");
                }
            }
        }

        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            StackFrameItem[] stackframes = loggingEvent.LocationInformation.StackFrames;
            if ((stackframes == null) || (stackframes.Length <= 0))
            {
                LogLog.Error(declaringType, "loggingEvent.LocationInformation.StackFrames was null or empty.");
                return;
            }

            int stackFrameIndex = m_stackFrameLevel - 1;
            while (stackFrameIndex >= 0)
            {
                if (stackFrameIndex >= stackframes.Length)
                {
                    stackFrameIndex--;
                    continue;
                }

                StackFrameItem stackFrame = stackframes[stackFrameIndex];
                writer.Write("{0}.{1}", stackFrame.ClassName, GetMethodInformation(stackFrame.Method));
                if (stackFrameIndex > 0)
                {
                    // TODO: make this user settable?
                    writer.Write(" > ");
                }
                stackFrameIndex--;
            }
        }

        internal virtual string GetMethodInformation(MethodItem method)
        {
            return method.Name;
        }

        private readonly static Type declaringType = typeof(StackTracePatternConverter);
    }
}
