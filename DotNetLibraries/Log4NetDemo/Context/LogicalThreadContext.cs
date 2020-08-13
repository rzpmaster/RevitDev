namespace Log4NetDemo.Context
{
    /// <summary>
    /// 逻辑线程上下文
    /// </summary>
    public sealed class LogicalThreadContext
    {
        private LogicalThreadContext() { }

        /// <summary>
        /// The thread context properties instance
        /// </summary>
        private readonly static LogicalThreadContextProperties s_properties = new LogicalThreadContextProperties();
        /// <summary>
        /// 逻辑线程上下文属性字典
        /// </summary>
        public static LogicalThreadContextProperties Properties
        {
            get { return s_properties; }
        }

        /// <summary>
        /// The thread context stacks instance
        /// </summary>
        private readonly static LogicalThreadContextStacks s_stacks = new LogicalThreadContextStacks(s_properties);
        /// <summary>
        /// 逻辑线程上下文储存信息栈
        /// </summary>
        public static LogicalThreadContextStacks Stacks
        {
            get { return s_stacks; }
        }
    }
}
