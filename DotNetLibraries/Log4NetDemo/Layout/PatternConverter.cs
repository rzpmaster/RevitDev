using Log4NetDemo.Layout.Data;
using Log4NetDemo.Repository;
using Log4NetDemo.Util;
using Log4NetDemo.Util.Collections;
using System.Collections;
using System.IO;
using System.Text;

namespace Log4NetDemo.Layout
{
    public abstract class PatternConverter
    {
        protected PatternConverter()
        {
        }

        abstract protected void Convert(TextWriter writer, object state);

        #region Public Instance Properties

        public virtual PatternConverter Next
        {
            get { return m_next; }
        }

        public virtual FormattingInfo FormattingInfo
        {
            get { return new FormattingInfo(m_min, m_max, m_leftAlign); }
            set
            {
                m_min = value.Min;
                m_max = value.Max;
                m_leftAlign = value.LeftAlign;
            }
        }

        public virtual string Option
        {
            get { return m_option; }
            set { m_option = value; }
        }

        public PropertiesDictionary Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        #endregion

        #region Public Instance Methods

        public virtual PatternConverter SetNext(PatternConverter patternConverter)
        {
            m_next = patternConverter;
            return m_next;
        }

        virtual public void Format(TextWriter writer, object state)
        {
            if (m_min < 0 && m_max == int.MaxValue)
            {
                // Formatting options are not in use
                Convert(writer, state);
            }
            else
            {
                string msg = null;
                int len;
                lock (m_formatWriter)
                {
                    m_formatWriter.Reset(c_renderBufferMaxCapacity, c_renderBufferSize);

                    Convert(m_formatWriter, state);

                    StringBuilder buf = m_formatWriter.GetStringBuilder();
                    len = buf.Length;
                    if (len > m_max)
                    {
                        msg = buf.ToString(len - m_max, m_max);
                        len = m_max;
                    }
                    else
                    {
                        msg = buf.ToString();
                    }
                }

                if (len < m_min)
                {
                    if (m_leftAlign)
                    {
                        writer.Write(msg);
                        SpacePad(writer, m_min - len);
                    }
                    else
                    {
                        SpacePad(writer, m_min - len);
                        writer.Write(msg);
                    }
                }
                else
                {
                    writer.Write(msg);
                }
            }
        }

        private static readonly string[] SPACES = { " ", "  ", "    ", "        ",			// 1,2,4,8 spaces
													"                ",						// 16 spaces
													"                                " };	// 32 spaces

        protected static void SpacePad(TextWriter writer, int length)
        {
            while (length >= 32)
            {
                writer.Write(SPACES[5]);
                length -= 32;
            }

            for (int i = 4; i >= 0; i--)
            {
                if ((length & (1 << i)) != 0)
                {
                    writer.Write(SPACES[i]);
                }
            }
        }

        #endregion

        #region protected Static Methods

        protected static void WriteDictionary(TextWriter writer, ILoggerRepository repository, IDictionary value)
        {
            WriteDictionary(writer, repository, value.GetEnumerator());
        }

        protected static void WriteDictionary(TextWriter writer, ILoggerRepository repository, IDictionaryEnumerator value)
        {
            writer.Write("{");

            bool first = true;

            // Write out all the dictionary key value pairs
            while (value.MoveNext())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.Write(", ");
                }
                WriteObject(writer, repository, value.Key);
                writer.Write("=");
                WriteObject(writer, repository, value.Value);
            }

            writer.Write("}");
        }

        protected static void WriteObject(TextWriter writer, ILoggerRepository repository, object value)
        {
            if (repository != null)
            {
                repository.RendererMap.FindAndRender(value, writer);
            }
            else
            {
                // Don't have a repository to render with so just have to rely on ToString
                if (value == null)
                {
                    writer.Write(SystemInfo.NullText);
                }
                else
                {
                    writer.Write(value.ToString());
                }
            }
        }

        #endregion

        private PatternConverter m_next;
        private int m_min = -1;
        private int m_max = int.MaxValue;
        private bool m_leftAlign = false;

        /// <summary>
        /// The option string to the converter
        /// </summary>
        private string m_option = null;

        private PropertiesDictionary properties;

        private ReusableStringWriter m_formatWriter = new ReusableStringWriter(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Initial buffer size
        /// </summary>
        private const int c_renderBufferSize = 256;

        /// <summary>
        /// Maximum buffer size before it is recycled
        /// </summary>
        private const int c_renderBufferMaxCapacity = 1024;
    }
}
