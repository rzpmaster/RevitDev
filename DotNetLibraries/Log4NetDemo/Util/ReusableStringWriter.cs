using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Util
{
    public class ReusableStringWriter : StringWriter
    {
        public ReusableStringWriter(IFormatProvider formatProvider) : base(formatProvider) { }

        protected override void Dispose(bool disposing)
        {
            // Do not close the writer
        }

        /// <summary>
        /// 重置 StringBuilder ,需要重用时使用
        /// </summary>
        /// <param name="maxCapacity">被 trim 前的最大缓冲</param>
        /// <param name="defaultSize">缓冲区的默认大小</param>
        /// <remarks>
		/// <para>
		/// Reset this string writer so that it can be reused.
		/// The internal buffers are cleared and reset.
		/// </para>
		/// </remarks>
        public void Reset(int maxCapacity, int defaultSize)
        {
            // Reset working string buffer
            StringBuilder sb = this.GetStringBuilder();

            sb.Length = 0;

            // Check if over max size
            if (sb.Capacity > maxCapacity)
            {
                sb.Capacity = defaultSize;
            }
        }
    }
}
