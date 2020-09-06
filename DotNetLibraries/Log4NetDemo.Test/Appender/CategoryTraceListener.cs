using System.Diagnostics;

namespace Log4NetDemo.Test.Appender
{
    public class CategoryTraceListener : TraceListener
    {
        private string lastCategory;

        public override void Write(string message)
        {
            // empty
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void Write(string message, string category)
        {
            lastCategory = category;
            base.Write(message, category);
        }

        public string Category
        {
            get { return lastCategory; }
        }
    }
}
