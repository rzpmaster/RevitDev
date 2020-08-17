using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;

namespace Log4NetDemo.Filter
{
    /// <summary>
    /// 过滤器基类
    /// </summary>
    public abstract class FilterSkeleton : IFilter, IOptionHandler
    {
        #region Implementation of IFilter

        private IFilter m_next;
        /// <summary>
        /// Points to the next filter in the filter chain.
        /// </summary>
        public IFilter Next { get => m_next; set => m_next = value; }

        public abstract FilterDecision Decide(LoggingEvent loggingEvent);

        #endregion

        #region Implementation of IOptionHandler

        public virtual void ActivateOptions()
        {
        }

        #endregion
    }
}
