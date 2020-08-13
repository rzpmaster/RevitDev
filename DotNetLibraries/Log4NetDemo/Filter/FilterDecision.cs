namespace Log4NetDemo.Filter
{
    public enum FilterDecision : int
    {
        /// <summary>
        /// The log event must be dropped immediately without 
        /// consulting with the remaining filters, if any, in the chain.
        /// </summary>
        Deny = -1,

        /// <summary>
        /// This filter is neutral with respect to the log event. 
        /// The remaining filters, if any, should be consulted for a final decision.
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// The log event must be logged immediately without 
        /// consulting with the remaining filters, if any, in the chain.
        /// </summary>
        Accept = 1,
    }
}
