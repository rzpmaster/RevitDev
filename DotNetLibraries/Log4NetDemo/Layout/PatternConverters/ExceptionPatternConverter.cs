using Log4NetDemo.Core.Data;
using System.IO;

namespace Log4NetDemo.Layout.PatternConverters
{
    internal sealed class ExceptionPatternConverter : PatternLayoutConverter
    {
        public ExceptionPatternConverter()
        {
            // This converter handles the exception
            IgnoresException = false;
        }

        override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject != null && Option != null && Option.Length > 0)
            {
                switch (Option.ToLower())
                {
                    case "message":
                        WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.Message);
                        break;
                    case "source":
                        WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.Source);
                        break;
                    case "stacktrace":
                        WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.StackTrace);
                        break;
                    case "targetsite":
                        WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.TargetSite);
                        break;
                    case "helplink":
                        WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.HelpLink);
                        break;
                    default:
                        // do not output SystemInfo.NotAvailableText
                        break;
                }
            }
            else
            {
                string exceptionString = loggingEvent.GetExceptionString();
                if (exceptionString != null && exceptionString.Length > 0)
                {
                    writer.WriteLine(exceptionString);
                }
                else
                {
                    // do not output SystemInfo.NotAvailableText
                }
            }
        }
    }
}
