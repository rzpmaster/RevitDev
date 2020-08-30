using Log4NetDemo.Appender.ErrorHandler;
using System;
using System.IO;

namespace Log4NetDemo.Util.TextWriters
{
    public class CountingQuietTextWriter : QuietTextWriter
    {
        public CountingQuietTextWriter(TextWriter writer, IErrorHandler errorHandler) : base(writer, errorHandler)
        {
            m_countBytes = 0;
        }

		public override void Write(char value)
		{
			try
			{
				base.Write(value);

				// get the number of bytes needed to represent the 
				// char using the supplied encoding.
				m_countBytes += this.Encoding.GetByteCount(new char[] { value });
			}
			catch (Exception e)
			{
				this.ErrorHandler.Error("Failed to write [" + value + "].", e, ErrorCode.WriteFailure);
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (count > 0)
			{
				try
				{
					base.Write(buffer, index, count);

					// get the number of bytes needed to represent the 
					// char array using the supplied encoding.
					m_countBytes += this.Encoding.GetByteCount(buffer, index, count);
				}
				catch (Exception e)
				{
					this.ErrorHandler.Error("Failed to write buffer.", e, ErrorCode.WriteFailure);
				}
			}
		}

		override public void Write(string str)
		{
			if (str != null && str.Length > 0)
			{
				try
				{
					base.Write(str);

					// get the number of bytes needed to represent the 
					// string using the supplied encoding.
					m_countBytes += this.Encoding.GetByteCount(str);
				}
				catch (Exception e)
				{
					this.ErrorHandler.Error("Failed to write [" + str + "].", e, ErrorCode.WriteFailure);
				}
			}
		}

		public long Count
		{
			get { return m_countBytes; }
			set { m_countBytes = value; }
		}

		private long m_countBytes;
	}
}
