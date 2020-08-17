namespace Log4NetDemo.Filter
{
    /// <summary>
    /// 过滤器对日志事件的策略枚举
    /// </summary>
    public enum FilterDecision : int
    {
        /// <summary>
        /// The log event must be dropped immediately without 
        /// consulting with the remaining filters, if any, in the chain.
        /// 拒绝的，必须立刻删除日志，无需咨询过滤器链表中的剩余元素
        /// </summary>
        Deny = -1,

        /// <summary>
        /// This filter is neutral with respect to the log event. 
        /// The remaining filters, if any, should be consulted for a final decision.
        /// 中立的，无所谓，不这个过滤器由决定
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// The log event must be logged immediately without 
        /// consulting with the remaining filters, if any, in the chain.
        /// 认可的，必须立即记录日志，无需咨询过滤器链表中的剩余元素
        /// </summary>
        Accept = 1,
    }
}
