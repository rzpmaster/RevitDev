using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using System.IO;

namespace Log4NetDemo.Layout
{
    /// <summary>
    /// 日志输出布局基类
    /// </summary>
    public abstract class LayoutSkeleton : ILayout, IOptionHandler
    {
        protected LayoutSkeleton() { }

        #region Implementation of ILayout

        /// <summary>
        /// 默认的格式是 "text/plain" ，子类要想修改，可以重写
        /// </summary>
        virtual public string ContentType
        {
            get { return "text/plain"; }
        }

        virtual public string Header
        {
            get { return m_header; }
            set { m_header = value; }
        }

        virtual public string Footer
        {
            get { return m_footer; }
            set { m_footer = value; }
        }

        virtual public bool IgnoresException
        {
            get { return m_ignoresException; }
            set { m_ignoresException = value; }
        }

        public string Format(LoggingEvent loggingEvent)
        {
            StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            Format(writer, loggingEvent);
            return writer.ToString();
        }

        abstract public void Format(TextWriter writer, LoggingEvent loggingEvent);

        #endregion

        #region Implementation of IOptionHandler

        public virtual void ActivateOptions()
        {
        }

        #endregion

        private string m_header = null;
        private string m_footer = null;
        private bool m_ignoresException = true;
    }
}
