using Log4NetDemo.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Layout
{
    public class LayoutSkeleton : ILayout
    {
        public string ContentType => throw new NotImplementedException();

        public string Header => throw new NotImplementedException();

        public string Footer => throw new NotImplementedException();

        public bool IgnoresException => throw new NotImplementedException();

        public void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            throw new NotImplementedException();
        }
    }
}
