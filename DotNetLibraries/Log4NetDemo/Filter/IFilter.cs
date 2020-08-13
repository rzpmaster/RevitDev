using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;

namespace Log4NetDemo.Filter
{
    public interface IFilter : IOptionHandler
    {
        FilterDecision Decide(LoggingEvent loggingEvent);

        IFilter Next { get; set; }
    }
}
