namespace Log4NetDemo.Core.Interface
{
    /// <summary>
    /// Logger包装器
    /// </summary>
    public interface ILoggerWrapper
    {
        ILogger Logger { get; }
    }
}
