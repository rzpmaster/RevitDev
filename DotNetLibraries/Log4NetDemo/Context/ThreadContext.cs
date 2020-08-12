namespace Log4NetDemo.Context
{
    /// <summary>
    /// 线程上下文
    /// </summary>
    public sealed class ThreadContext
    {
        private ThreadContext() { }

        private readonly static ThreadContextProperties s_properties = new ThreadContextProperties();
        /// <summary>
        /// 线程上下文属性字典
        /// </summary>
        public static ThreadContextProperties Properties
        {
            get { return s_properties; }
        }

        private readonly static ThreadContextStacks s_stacks = new ThreadContextStacks(s_properties);
        /// <summary>
        /// 线程上下文储存信息栈
        /// </summary>
        public static ThreadContextStacks Stacks
        {
            get { return s_stacks; }
        }


    }
}
