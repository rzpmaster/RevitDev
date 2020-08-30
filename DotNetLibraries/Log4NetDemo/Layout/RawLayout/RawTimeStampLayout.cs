using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Layout.RawLayout
{
    public class RawTimeStampLayout : IRawLayout
    {
        public virtual object Format(LoggingEvent loggingEvent)
        {
            return loggingEvent.TimeStamp;
        }
    }
}
