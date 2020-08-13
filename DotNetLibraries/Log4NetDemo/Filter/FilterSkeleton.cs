using Log4NetDemo.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Filter
{
    public class FilterSkeleton : IFilter
    {
        public IFilter Next { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ActivateOptions()
        {
            throw new NotImplementedException();
        }

        public FilterDecision Decide(LoggingEvent loggingEvent)
        {
            throw new NotImplementedException();
        }
    }
}
