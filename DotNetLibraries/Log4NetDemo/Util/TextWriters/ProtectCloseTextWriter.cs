using System.IO;

namespace Log4NetDemo.Util.TextWriters
{
    public class ProtectCloseTextWriter : TextWriterAdapter
    {
        public ProtectCloseTextWriter(TextWriter writer) : base(writer)
        {
        }

        public void Attach(TextWriter writer)
        {
            this.Writer = writer;
        }

        override public void Close()
        {
            // do nothing
        }
    }

}
