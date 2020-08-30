using Log4NetDemo.Core.Data;
using System;
using System.IO;

namespace Log4NetDemo.Layout
{
    public class SimpleLayout : LayoutSkeleton
    {
        public SimpleLayout()
        {
            IgnoresException = true;
        }

        override public void ActivateOptions()
        {
            // nothing to do.
        }

        override public void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            writer.Write(loggingEvent.Level.DisplayName);
            writer.Write(" - ");
            loggingEvent.WriteRenderedMessage(writer);
            writer.WriteLine();
        }
    }
}
