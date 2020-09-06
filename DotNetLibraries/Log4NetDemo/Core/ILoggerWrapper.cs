namespace Log4NetDemo.Core
{
    /// <summary>
    /// 所有 Logger 包装器的基类
    /// </summary>
    public interface ILoggerWrapper
    {
        /// <summary>
        /// 要包装的对象
        /// </summary>
        ILogger Logger { get; }
    }
}
