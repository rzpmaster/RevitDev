using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Log4NetDemo.Util.TextWriters
{
    public abstract class TextWriterAdapter : TextWriter
    {
        private TextWriter m_writer;
        protected TextWriterAdapter(TextWriter writer) : base(CultureInfo.InvariantCulture)
        {
            m_writer = writer;
        }

        protected TextWriter Writer
        {
            get { return m_writer; }
            set { m_writer = value; }
        }

        override public Encoding Encoding
        {
            get { return m_writer.Encoding; }
        }

        override public IFormatProvider FormatProvider
        {
            get { return m_writer.FormatProvider; }
        }

        override public String NewLine
        {
            get { return m_writer.NewLine; }
            set { m_writer.NewLine = value; }
        }

        override public void Close()
        {
            m_writer.Close();
        }

        override protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)m_writer).Dispose();
            }
        }

        override public void Flush()
        {
            m_writer.Flush();
        }

        override public void Write(char value)
        {
            m_writer.Write(value);
        }

        override public void Write(char[] buffer, int index, int count)
        {
            m_writer.Write(buffer, index, count);
        }

        override public void Write(String value)
        {
            m_writer.Write(value);
        }
    }
}
